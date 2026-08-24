#if SWITCH
using Avalonia.VisualTree;
using System.Net.NetworkInformation;
using Tkmm.Actions;
using Tkmm.Core;
using Tkmm.Models.MenuModels;
using Tkmm.ViewModels.Pages;
using Tkmm.Views.Pages;
using Tkmm.Wizard.Helpers;
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

        if (NetworkInterface.GetIsNetworkAvailable()) {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await SystemActions.CheckForUpdates(isUserInvoked: false, cts.Token);
        }

        return TkKeyUtils.TryGetKeys(TkConfig.Shared.SdCardRootPath, out _)
            ? await VerifyDump(wizard)
            : StepResult.Next(WizardSteps.MissingKeys);
    }

    public static async ValueTask<StepResult> MissingKeys(SetupWizard wizard)
        => await RebootPrompt(wizard,
            TkLocale.SetupWizard_MissingKeys_Title,
            TkLocale.SetupWizard_MissingKeys_Content);

    private static async ValueTask<StepResult> VerifyDump(SetupWizard wizard)
    {
        if ((await TkRomHelper.Validate()).Ok) {
            return FlowHelper.AfterDump();
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
                .WithAction(TkLocale.Menu_NxReboot)
                .Show()) {
            return StepResult.Back();
        }

        NxMenuModel.Reboot();
        await Task.Delay(-1);
        return StepResult.Done();
    }
}
#endif
