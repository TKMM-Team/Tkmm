#if !SWITCH
using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Dialogs;
using Tkmm.Wizard.Pages;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Wizard.WizardPages;

internal static class ManualDumpPages
{
    public static async ValueTask<StepResult> Show(DesktopSetupWizard wizard)
        => await Run(wizard, wizard.DumpSource, wizard.EmulatorPathHint)
            ? DesktopWizardPages.AfterDump()
            : StepResult.Back();

    private static async ValueTask<bool> Run(SetupWizard wizard, DumpSource dumpSource, string? pathHint)
    {
        while (true) {
            if (dumpSource != DumpSource.Switch) {
                var (ok, next) = await ConfigureEmulator(wizard, pathHint);
                if (!ok) {
                    return false;
                }

                pathHint = next;
            }

            if (TKMM.TryGetTkRom(out var hasBase, out var hasUpdate, out _) is null
                && !await ConfigureDump(wizard, hasBase, hasUpdate)) {
                if (dumpSource is DumpSource.Switch) {
                    return false;
                }

                continue;
            }

            if (!await ConfigureMergeOutput(wizard, dumpSource)) {
                continue;
            }

            return true;
        }
    }

    private static async ValueTask<bool> ConfigureDump(SetupWizard wizard, bool hasBase, bool hasUpdate)
    {
        var isRomfs = false;
        if (!hasBase) {
            var (baseOk, romfs) = await ConfigureBase(wizard);
            if (!baseOk) {
                return false;
            }

            isRomfs = romfs;
        }

        if (isRomfs) {
            if (TKMM.TryGetTkRom(out var error) is not null) {
                return true;
            }

            if (error is not null) {
                await MessageDialog.Show(error, TkLocale.TkExtensibleRomProvider_InvalidGameDump);
            }

            return false;
        }

        if (!await ValidateRom(needUpdate: false)) {
            return false;
        }

        TKMM.TryGetTkRom(out _, out hasUpdate, out _);
        return hasUpdate
               || (await ConfigureUpdate(wizard) && await ValidateRom(needUpdate: true));
    }

    private static async ValueTask<(bool Ok, string? Hint)> ConfigureEmulator(SetupWizard wizard, string? hint)
    {
        TkConfig.Shared.ResetGameDumpSettings();
        Config.Shared.MergeOutput = null;

        EmulatorNameInputPageContext ctx = new() { EmulatorName = hint ?? string.Empty };
        if (!await wizard.NextPage()
                .WithTitle(TkLocale.SetupWizard_EmulatorNameInput_Title)
                .WithContent<EmulatorNameInputPage>(ctx)
                .Show()) {
            return (false, hint);
        }

        try {
            Config.Shared.EmulatorPath = ctx.EmulatorName;
            if (Path.GetFileNameWithoutExtension(ctx.EmulatorName)
                .Equals("ryujinx", StringComparison.OrdinalIgnoreCase)) {
                TkRyujinxHelper.UseRyujinx(out _, true);
            }
            else {
                TkEmulatorHelper.UseEmulator(ctx.EmulatorName, out _);
            }
        }
        catch {
            // Continue with dump setup
        }

        return (true, ctx.EmulatorName);
    }

    private static async ValueTask<(bool Ok, bool IsRomfs)> ConfigureBase(SetupWizard wizard)
    {
        TkConfig.Shared.GameDumpFolderPaths.Clear();
        TkConfig.Shared.PackagedBaseGamePaths.Clear();

        var (next, selected) = await wizard.ChooseAsync(
            TkLocale.SetupWizard_DumpType_Title,
            [
                SetupWizard.Opt(Locale[TkLocale.SetupWizard_DumpType_XciNsp], BaseGameDumpType.XciNsp, selected: true),
                SetupWizard.Opt(Locale[TkLocale.SetupWizard_DumpType_Romfs], BaseGameDumpType.Romfs),
                SetupWizard.Opt(Locale[TkLocale.SetupWizard_DumpType_SdCard], BaseGameDumpType.SdCard),
                SetupWizard.Opt(Locale[TkLocale.SetupWizard_DumpType_Nand], BaseGameDumpType.Nand)
            ],
            "baseGameDumpType",
            Locale[TkLocale.SetupWizard_DumpType_Description]);

        if (!next) {
            return (false, false);
        }

        var type = selected?.Tag is BaseGameDumpType t ? t : BaseGameDumpType.XciNsp;
        var ok = await (type switch {
            BaseGameDumpType.Romfs => SetupWizard.ApplyFolder(
                Locale[TkLocale.SetupWizard_SelectRomfsFolder],
                p => TkConfig.Shared.GameDumpFolderPaths.New(p)),
            BaseGameDumpType.SdCard => ApplySdCard(),
            BaseGameDumpType.Nand => ApplyNand(wizard),
            _ => ConfigureXciNsp(wizard)
        });

        return (ok, ok && type is BaseGameDumpType.Romfs);
    }

    private static async ValueTask<bool> ConfigureUpdate(SetupWizard wizard)
    {
        TkConfig.Shared.PackagedUpdatePaths.Clear();

        var (next, selected) = await wizard.ChooseAsync(
            TkLocale.SetupWizard_UpdateDumpType_Title,
            [
                SetupWizard.Opt(Locale[TkLocale.SetupWizard_DumpType_Nsp], UpdateDumpType.Nsp, selected: true),
                SetupWizard.Opt(Locale[TkLocale.SetupWizard_DumpType_SdCard], UpdateDumpType.SdCard),
                SetupWizard.Opt(Locale[TkLocale.SetupWizard_DumpType_Nand], UpdateDumpType.Nand)
            ],
            "updateDumpType",
            Locale[TkLocale.SetupWizard_UpdateDumpType_Description]);

        if (!next) {
            return false;
        }

        switch (selected?.Tag is UpdateDumpType u ? u : UpdateDumpType.Nsp) {
            case UpdateDumpType.SdCard:
                await ApplySdCard();
                break;
            case UpdateDumpType.Nand:
                await ApplyNand(wizard);
                break;
            default:
                if (await ConfigureKeys(wizard)
                    && await SetupWizard.PickFileAsync(
                        Locale[TkLocale.SetupWizard_SelectUpdateNspFile], "NSP", "*.nsp") is { } path) {
                    TkConfig.Shared.PackagedUpdatePaths.New(path);
                }

                break;
        }

        return true;
    }

    private static async ValueTask<bool> ConfigureMergeOutput(SetupWizard wizard, DumpSource dumpSource)
    {
        if (!string.IsNullOrEmpty(Config.Shared.MergeOutput)
            || dumpSource is DumpSource.Switch
            || Config.Shared.TkmmMode.IsSwitch) {
            return true;
        }

        var (next, path) = await wizard.ShowFolderPathAsync(
            TkLocale.Config_MergeOutputFolder,
            Locale[TkLocale.SetupWizard_MergeOutputSetup_Description],
            Locale[TkLocale.SetupWizard_MergeOutputSetup_Path],
            header: Locale[TkLocale.SetupWizard_MergeOutputSetup_Path]);

        if (!next) {
            return false;
        }

        path ??= string.Empty;
        if (Path.GetFileNameWithoutExtension(path)
            .Equals("0100f2c0115b6000", StringComparison.OrdinalIgnoreCase)) {
            path = Path.Combine(path, "TKMM");
        }

        Config.Shared.MergeOutput = path;
        Config.Shared.Save();
        return true;
    }

    private static async ValueTask<bool> ConfigureKeys(SetupWizard wizard)
    {
        if (HasValidKeys(TkConfig.Shared.KeysFolderPath)) {
            return true;
        }

        while (true) {
            var (next, path) = await wizard.ShowFolderPathAsync(
                TkLocale.SetupWizard_KeysFolder_Title,
                Locale[TkLocale.SetupWizard_KeysFolder_Description],
                Locale[TkLocale.SetupWizard_SelectKeysFolder],
                initialPath: TkConfig.Shared.KeysFolderPath);

            if (!next) {
                return false;
            }

            if (!HasValidKeys(path)) {
                await MessageDialog.Show(
                    TkLocale.SetupWizard_ManualSetup_MissingKeys_Content,
                    TkLocale.SetupWizard_MissingKeys_Title);
                continue;
            }

            TkConfig.Shared.KeysFolderPath = path!;
            TkConfig.Shared.Save();
            return true;
        }
    }

    private static bool HasValidKeys(string? path)
        => path is not null && Directory.Exists(path) && TkKeyUtils.GetKeysFromFolder(path) is not null;

    private static async ValueTask<bool> ConfigureXciNsp(SetupWizard wizard)
    {
        while (true) {
            if (!await ConfigureKeys(wizard)) {
                return false;
            }

            var (next, selected) = await wizard.ChooseAsync(
                TkLocale.SetupWizard_BaseGameSplit_Title,
                [
                    SetupWizard.Opt(Locale[TkLocale.SetupWizard_BaseGameSplit_SingleFile], false, selected: true),
                    SetupWizard.Opt(Locale[TkLocale.SetupWizard_BaseGameSplit_SplitFolder], true)
                ],
                "baseGameSplit");

            if (!next) {
                return false;
            }

            if (selected?.Tag is true) {
                if (await SetupWizard.ApplyFolder(
                        Locale[TkLocale.SetupWizard_SelectSplitFilesFolder],
                        p => TkConfig.Shared.PackagedBaseGamePaths.New(p))) {
                    return true;
                }
            }
            else if (await SetupWizard.PickFileAsync(
                         Locale[TkLocale.SetupWizard_SelectBaseGameFile], "XCI/NSP", "*.xci", "*.nsp") is { } file) {
                TkConfig.Shared.PackagedBaseGamePaths.New(file);
                return true;
            }
        }
    }

    private static ValueTask<bool> ApplySdCard()
        => SetupWizard.ApplyFolder(Locale[TkLocale.SetupWizard_SelectSdCardRoot], path => {
            TkConfig.Shared.SdCardRootPath = path;
            TkConfig.Shared.KeysFolderPath = Path.Combine(path, "switch");
        });

    private static async ValueTask<bool> ApplyNand(SetupWizard wizard)
        => await ConfigureKeys(wizard)
           && await SetupWizard.ApplyFolder(
               Locale[TkLocale.SetupWizard_SelectNandFolder],
               p => TkConfig.Shared.NandFolderPaths.New(p));

    private static async ValueTask<bool> ValidateRom(bool needUpdate)
    {
        var rom = TKMM.TryGetTkRom(out var hasBase, out var hasUpdate, out _);
        if (rom is not null || (needUpdate ? hasUpdate : hasBase)) {
            return true;
        }

        await MessageDialog.Show(
            Locale[needUpdate
                ? TkLocale.SetupWizard_UpdateDumpConfigPage_InvalidConfiguration
                : TkLocale.SetupWizard_BaseGameDumpConfigPage_InvalidConfiguration],
            needUpdate
                ? TkLocale.SetupWizard_UpdateDumpConfigPage_InvalidConfiguration_Title
                : TkLocale.SetupWizard_BaseGameDumpConfigPage_InvalidConfiguration_Title);

        if (needUpdate) {
            TkConfig.Shared.PackagedUpdatePaths.Clear();
        }
        else {
            TkConfig.Shared.PackagedBaseGamePaths.Clear();
        }

        return false;
    }
}
#endif