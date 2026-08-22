#if SWITCH
using Avalonia.VisualTree;
using Tkmm.Core;
using Tkmm.Dialogs;
using Tkmm.Models.MenuModels;
using Tkmm.ViewModels.Pages;
using Tkmm.Views.Pages;
using Tkmm.Wizard.Models;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Wizard.Steps;

internal static class NxSteps
{
    public static async ValueTask<StepResult> Wifi(SetupWizard wizard)
    {
        var windowHeight = wizard.Presenter.FindAncestorOfType<Avalonia.Controls.Window>()?.Height ?? 720.0;
        var networkPage = new NetworkSettingsPageView {
            MaxHeight = windowHeight * 0.62,
            DataContext = NetworkSettingsPageViewModel.Shared
        };

        if (!await wizard.NextPage()
                .WithTitle(TkLocale.NetworkSettings_WiFiService_Name)
                .WithContent(networkPage)
                .Show()) {
            return StepResult.Back();
        }

        if (!TkKeyUtils.TryGetKeys(TkConfig.Shared.SdCardRootPath, out _)) {
            return StepResult.Next(WizardSteps.MissingKeys);
        }

        return TKMM.TryGetTkRom(out _) is not null
            ? StepResult.Next(WizardSteps.Firmware)
            : StepResult.Next(WizardSteps.VerifyDump);
    }

    public static async ValueTask<StepResult> MissingKeys(SetupWizard wizard)
        => await RebootPrompt(wizard,
            TkLocale.SetupWizard_MissingKeys_Title,
            TkLocale.SetupWizard_MissingKeys_Content);

    public static async ValueTask<StepResult> VerifyDump(SetupWizard wizard)
    {
        if (TKMM.TryGetTkRom(out string? error) is not null) {
            return StepResult.Next(WizardSteps.Firmware);
        }

        if (error is not null) {
            await MessageDialog.Show(error, TkLocale.SetupWizard_GameDumpConfigPage_InvalidConfiguration_Title);
        }

        return await RebootPrompt(wizard,
            TkLocale.SetupWizard_MissingDump_Title,
            TkLocale.SetupWizard_MissingDump_Content);
    }

    private static async ValueTask<StepResult> RebootPrompt(SetupWizard wizard, TkLocale title, TkLocale content)
    {
        if (!await wizard.NextPage()
                .WithTitle(title)
                .WithContent(content)
                .WithActionContent(TkLocale.Menu_NxReboot)
                .Show()) {
            return StepResult.Back();
        }

        NxMenuModel.Reboot();
        await Task.Delay(-1);
        return StepResult.Done();
    }
}
#endif
