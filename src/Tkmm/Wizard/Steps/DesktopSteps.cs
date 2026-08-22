#if !SWITCH
using System.Runtime.InteropServices;
using FluentAvalonia.UI.Controls;
using Tkmm.Core;
using Tkmm.Core.Models;
using Tkmm.Dialogs;
using Tkmm.Wizard.Helpers;
using Tkmm.Wizard.Models;
using Tkmm.Wizard.Pages;

namespace Tkmm.Wizard.Steps;

internal static class DesktopSteps
{
    public static async ValueTask<StepResult> Mode(SetupWizard wizard)
    {
        var (next, selected) = await wizard.NextPage()
            .WithTitle(TkLocale.SetupWizard_TkmmMode_Title)
            .WithDescription(TkLocale.SetupWizard_TkmmMode_Description)
            .WithOptions([
                WizardRadioOption.Opt(TkLocale.SetupWizard_TkmmMode_Emulator, new TkmmMode("Emulator"), selected: true),
                WizardRadioOption.Opt(TkLocale.SetupWizard_TkmmMode_NintendoSwitch, new TkmmMode("Switch")),
                WizardRadioOption.Opt(TkLocale.SetupWizard_TkmmMode_Both, new TkmmMode("Hybrid"))])
            .WithGroupName("tkmmMode")
            .Show();

        if (!next) {
            return StepResult.Back();
        }

        if (selected?.Tag is not TkmmMode mode) {
            return StepResult.Next(WizardSteps.Mode);
        }

        Config.Shared.TkmmMode = mode;
        if (mode.IsSwitch) {
            Config.Shared.MergeOutput = null;
            return StepResult.Next(WizardSteps.NxRecommend);
        }

        if (mode.IsEmulator) {
            Config.Shared.SwitchFirmwareVersion = Config.FirmwareVersions[0];
        }

        return StepResult.Next(WizardSteps.DumpSource);
    }

    public static async ValueTask<StepResult> NxRecommend(SetupWizard wizard)
    {
        if (!await wizard.NextPage()
                .WithTitle(TkLocale.SetupWizard_TkmmNx_Title)
                .WithContent<TkmmNxRecommendPage>()
                .Show()) {
            return StepResult.Back();
        }

        wizard.SelectedDumpSource = DumpSource.Switch;
        return StepResult.Next(WizardSteps.Manual);
    }

    public static async ValueTask<StepResult> DumpSourceStep(SetupWizard wizard)
    {
        var isIntelMac = RuntimeInformation.OSArchitecture is Architecture.X64 && OperatingSystem.IsMacOS();
        var (next, selected) = await wizard.NextPage()
            .WithTitle(TkLocale.SetupWizard_DumpSource_Title)
            .WithOptions([
                new WizardRadioOption {
                    Content = Locale[TkLocale.SetupWizard_DumpSource_RyujinxOption],
                    IsSelected = !isIntelMac,
                    IsEnabled = !isIntelMac,
                    Tag = DumpSource.Ryujinx
                },
                WizardRadioOption.Opt(TkLocale.SetupWizard_DumpSource_OtherOption, DumpSource.Other, selected: isIntelMac),
                new WizardRadioOption {
                    Content = Locale[TkLocale.SetupWizard_DumpSource_SwitchOption],
                    IsVisible = Config.Shared.TkmmMode.IsHybrid,
                    Tag = DumpSource.Switch
                }
            ])
            .WithGroupName("dumpSource")
            .Show();

        if (!next) {
            return StepResult.Back();
        }

        if (selected?.Tag is not DumpSource source) {
            await MessageDialog.Show(
                TkLocale.SetupWizard_Popup_InvalidDumpSource_Content,
                TkLocale.SetupWizard_Popup_InvalidDumpSource_Title);
            return StepResult.Next(WizardSteps.DumpSource);
        }

        wizard.SelectedDumpSource = source;
        wizard.EmulatorPathHint = null;
        return source is DumpSource.Ryujinx
            ? StepResult.Next(WizardSteps.Ryujinx)
            : StepResult.Next(WizardSteps.Manual);
    }

    public static async ValueTask<StepResult> Ryujinx(SetupWizard wizard)
    {
        if (!await wizard.NextPage()
                .WithTitle(TkLocale.SetupWizard_RyujinxSetup_Title)
                .WithContent(TkLocale.SetupWizard_RyujinxSetup_Content)
                .WithActionContent(TkLocale.SetupWizard_RyujinxSetup_Action)
                .Show()) {
            return StepResult.Back();
        }

        if (EmulatorSetupHelper.TryUseRunningRyujinx() is { } error) {
            var errorResult = await ErrorDialog.ShowAsync(new Exception(error), forceShowInDebug: true,
                TaskDialogStandardResult.Retry, TaskDialogStandardResult.Cancel);

            if (errorResult is TaskDialogStandardResult.Retry) {
                return StepResult.Next(WizardSteps.Ryujinx);
            }

            wizard.SelectedDumpSource = DumpSource.Other;
            wizard.EmulatorPathHint = "ryujinx";
            return StepResult.Next(WizardSteps.Manual);
        }

        if (TKMM.TryGetTkRom(out var romError) is null) {
            await MessageDialog.Show(
                romError ?? Locale[TkLocale.SetupWizard_GameDumpConfigPage_InvalidConfiguration],
                TkLocale.SetupWizard_GameDumpConfigPage_InvalidConfiguration_Title);
        }

        return AfterDump();
    }

    public static StepResult AfterDump()
        => Config.Shared.TkmmMode.IsSwitch || Config.Shared.TkmmMode.IsHybrid
            ? StepResult.Next(WizardSteps.Firmware)
            : StepResult.Next(WizardSteps.GameLanguage);
}
#endif
