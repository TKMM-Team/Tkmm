#if !SWITCH
using Tkmm.Core;
using Tkmm.Dialogs;
using Tkmm.Wizard.Helpers;
using Tkmm.Wizard.Models;
using Tkmm.Wizard.Pages;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Wizard.Steps;

internal static class ManualSteps
{
    public static async ValueTask<StepResult> Show(SetupWizard wizard)
        => await Run(wizard, wizard.SelectedDumpSource, wizard.EmulatorPathHint)
            ? DesktopSteps.AfterDump()
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

            if (TKMM.TryGetTkRom(out var hasBase, out _, out _) is null
                && !await ConfigureDump(wizard, hasBase)) {
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

    private static async ValueTask<bool> ConfigureDump(SetupWizard wizard, bool hasBase)
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

        var (ok, hasUpdate) = await ValidateRom(needUpdate: false);
        if (!ok) {
            return false;
        }

        return hasUpdate || (await ConfigureUpdate(wizard) && (await ValidateRom(needUpdate: true)).Ok);
    }

    private static async ValueTask<(bool Ok, string? Hint)> ConfigureEmulator(SetupWizard wizard, string? hint)
    {
        EmulatorSetupHelper.ResetDumpConfiguration();

        EmulatorNameInputPageContext ctx = new() { EmulatorName = hint ?? string.Empty };
        if (!await wizard.NextPage()
                .WithTitle(TkLocale.SetupWizard_EmulatorNameInput_Title)
                .WithContent<EmulatorNameInputPage>(ctx)
                .Show()) {
            return (false, hint);
        }

        try {
            EmulatorSetupHelper.ApplyFromNameOrPath(ctx.EmulatorName);
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

        var (next, selected) = await wizard.NextPage()
            .WithTitle(TkLocale.SetupWizard_DumpType_Title)
            .WithDescription(TkLocale.SetupWizard_DumpType_Description)
            .WithOptions([
                WizardRadioOption.Opt(TkLocale.SetupWizard_DumpType_XciNsp, BaseGameDumpType.XciNsp, selected: true),
                WizardRadioOption.Opt(TkLocale.SetupWizard_DumpType_Romfs, BaseGameDumpType.Romfs),
                WizardRadioOption.Opt(TkLocale.SetupWizard_DumpType_SdCard, BaseGameDumpType.SdCard),
                WizardRadioOption.Opt(TkLocale.SetupWizard_DumpType_Nand, BaseGameDumpType.Nand)])
            .WithGroupName("baseGameDumpType")
            .Show();

        if (!next) {
            return (false, false);
        }

        var type = selected?.Tag is BaseGameDumpType t ? t : BaseGameDumpType.XciNsp;
        var ok = await (type switch {
            BaseGameDumpType.Romfs => WizardStorageHelper.ApplyFolder(
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

        var (next, selected) = await wizard.NextPage()
            .WithTitle(TkLocale.SetupWizard_UpdateDumpType_Title)
            .WithDescription(TkLocale.SetupWizard_UpdateDumpType_Description)
            .WithOptions([
                WizardRadioOption.Opt(TkLocale.SetupWizard_DumpType_Nsp, UpdateDumpType.Nsp, selected: true),
                WizardRadioOption.Opt(TkLocale.SetupWizard_DumpType_SdCard, UpdateDumpType.SdCard),
                WizardRadioOption.Opt(TkLocale.SetupWizard_DumpType_Nand, UpdateDumpType.Nand)])
            .WithGroupName("updateDumpType")
            .Show();

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
            case UpdateDumpType.Nsp:
            default:
                if (await ConfigureKeys(wizard)
                    && await WizardStorageHelper.PickFileAsync(
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

        var result = await wizard.NextPage()
            .WithTitle(TkLocale.Config_MergeOutputFolder)
            .WithDescription(TkLocale.SetupWizard_MergeOutputSetup_Description)
            .WithFolderPicker(
                browseTitle: TkLocale.SetupWizard_MergeOutputSetup_Path,
                header: TkLocale.SetupWizard_MergeOutputSetup_Path)
            .Show();

        if (!result) {
            return false;
        }

        var path = result.Path ?? string.Empty;
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
            var result = await wizard.NextPage()
                .WithTitle(TkLocale.SetupWizard_KeysFolder_Title)
                .WithDescription(TkLocale.SetupWizard_KeysFolder_Description)
                .WithFolderPicker(
                    browseTitle: TkLocale.SetupWizard_SelectKeysFolder,
                    initialPath: TkConfig.Shared.KeysFolderPath)
                .Show();

            if (!result) {
                return false;
            }

            var path = result.Path;
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

            var (next, selected) = await wizard.NextPage()
                .WithTitle(TkLocale.SetupWizard_BaseGameSplit_Title)
                .WithOptions([
                    WizardRadioOption.Opt(TkLocale.SetupWizard_BaseGameSplit_SingleFile, false, selected: true),
                    WizardRadioOption.Opt(TkLocale.SetupWizard_BaseGameSplit_SplitFolder, true)])
                .WithGroupName("baseGameSplit")
                .Show();

            if (!next) {
                return false;
            }

            if (selected?.Tag is true) {
                if (await WizardStorageHelper.ApplyFolder(
                        Locale[TkLocale.SetupWizard_SelectSplitFilesFolder],
                        p => TkConfig.Shared.PackagedBaseGamePaths.New(p))) {
                    return true;
                }
            }
            else if (await WizardStorageHelper.PickFileAsync(
                         Locale[TkLocale.SetupWizard_SelectBaseGameFile], "XCI/NSP", "*.xci", "*.nsp") is { } file) {
                TkConfig.Shared.PackagedBaseGamePaths.New(file);
                return true;
            }
        }
    }

    private static ValueTask<bool> ApplySdCard()
        => WizardStorageHelper.ApplyFolder(Locale[TkLocale.SetupWizard_SelectSdCardRoot], path => {
            TkConfig.Shared.SdCardRootPath = path;
            TkConfig.Shared.KeysFolderPath = Path.Combine(path, "switch");
        });

    private static async ValueTask<bool> ApplyNand(SetupWizard wizard)
        => await ConfigureKeys(wizard)
           && await WizardStorageHelper.ApplyFolder(
               Locale[TkLocale.SetupWizard_SelectNandFolder],
               p => TkConfig.Shared.NandFolderPaths.New(p));

    private static async ValueTask<(bool Ok, bool HasUpdate)> ValidateRom(bool needUpdate)
    {
        var rom = TKMM.TryGetTkRom(out var hasBase, out var hasUpdate, out _);
        if (rom is not null || (needUpdate ? hasUpdate : hasBase)) {
            return (true, rom is not null || hasUpdate);
        }

        var (content, title) = needUpdate
            ? (TkLocale.SetupWizard_UpdateDumpConfigPage_InvalidConfiguration,
                TkLocale.SetupWizard_UpdateDumpConfigPage_InvalidConfiguration_Title)
            : (TkLocale.SetupWizard_BaseGameDumpConfigPage_InvalidConfiguration,
                TkLocale.SetupWizard_BaseGameDumpConfigPage_InvalidConfiguration_Title);
        await MessageDialog.Show(content, title);

        if (needUpdate) {
            TkConfig.Shared.PackagedUpdatePaths.Clear();
        }
        else {
            TkConfig.Shared.PackagedBaseGamePaths.Clear();
        }

        return (false, false);
    }
}
#endif
