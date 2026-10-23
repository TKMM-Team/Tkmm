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
            ? FlowHelper.AfterDump()
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

            if (!await ConfigureMergeOutput(wizard)) {
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
            return (await TkRomHelper.Validate(isRomfs: true)).Ok;
        }

        var (ok, isComplete) = await TkRomHelper.Validate(needUpdate: false);
        if (!ok) {
            return false;
        }

        return isComplete || (await ConfigureUpdate(wizard) && (await TkRomHelper.Validate()).Ok);
    }

    private static async ValueTask<(bool Ok, string? Hint)> ConfigureEmulator(SetupWizard wizard, string? hint)
    {
        EmulatorHelper.ResetConfiguration();

        EmulatorNameInputPageContext ctx = new() { EmulatorName = hint ?? string.Empty };
        if (!await wizard.NextPage()
                .WithTitle(TkLocale.SetupWizard_EmulatorNameInput_Title)
                .WithContent<EmulatorNameInputPage>(ctx)
                .Show()) {
            return (false, hint);
        }

        try {
            EmulatorHelper.ApplyFromNameOrPath(ctx.EmulatorName);
        }
        catch {
            // Continue with dump setup
            // TODO: show a dialog like the ryu setup page: "Continue with manual setup?" + Retry and Yes buttons
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
            BaseGameDumpType.Romfs => StorageHelper.ApplyFolder(
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
            default: {
                if (await ConfigureKeys(wizard)) {
                    foreach (var path in await StorageHelper.PickFilesAsync(
                                 Locale[TkLocale.SetupWizard_SelectUpdateNspFile], "NSP", "*.nsp")) {
                        TkConfig.Shared.PackagedUpdatePaths.New(path);
                    }
                }
                break;
            }
        }

        return true;
    }

    private static async ValueTask<bool> ConfigureMergeOutput(SetupWizard wizard)
    {
        if (!string.IsNullOrEmpty(Config.Shared.MergeOutput) || Config.Shared.TkmmMode.IsSwitch) {
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
                    WizardRadioOption.Opt(TkLocale.SetupWizard_BaseGameSplit_SingleFile, tag: false, selected: true),
                    WizardRadioOption.Opt(TkLocale.SetupWizard_BaseGameSplit_SplitFolder, tag: true)])
                .WithGroupName("baseGameSplit")
                .Show();

            if (!next) {
                return false;
            }

            if (selected?.Tag is true) {
                if (await StorageHelper.ApplyFolder(
                        Locale[TkLocale.SetupWizard_SelectSplitFilesFolder],
                        p => TkConfig.Shared.PackagedBaseGamePaths.New(p))) {
                    return true;
                }
            }
            else if (await StorageHelper.PickFileAsync(
                         Locale[TkLocale.SetupWizard_SelectBaseGameFile], "XCI/NSP", "*.xci", "*.nsp") is { } file) {
                TkConfig.Shared.PackagedBaseGamePaths.New(file);
                return true;
            }
        }
    }

    private static ValueTask<bool> ApplySdCard()
        => StorageHelper.ApplyFolder(Locale[TkLocale.SetupWizard_SelectSdCardRoot], path => {
            TkConfig.Shared.SdCardRootPath = path;
            TkConfig.Shared.KeysFolderPath = Path.Combine(path, "switch");
        });

    private static async ValueTask<bool> ApplyNand(SetupWizard wizard)
        => await ConfigureKeys(wizard)
           && await StorageHelper.ApplyFolder(
               Locale[TkLocale.SetupWizard_SelectNandFolder],
               p => TkConfig.Shared.NandFolderPaths.New(p));
}
#endif
