using System.Diagnostics;
using Avalonia.Controls.Notifications;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaMark;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Octokit;
using Tkmm.Core;
using Tkmm.Dialogs;
using Tkmm.Helpers;
using TkSharp.Core;

namespace Tkmm.Actions;

public sealed partial class SystemActions : GuardedActionGroup<SystemActions>
{
    protected override string ActionGroupName { get; } = nameof(SystemActions).Humanize();

    [RelayCommand]
    public static async Task ShowAboutDialog()
    {
        await using Stream aboutFileStream = AssetLoader.Open(new Uri("avares://Tkmm/Assets/About.md"));
        string contents = await new StreamReader(aboutFileStream).ReadToEndAsync();

        contents = contents.Replace("@@version@@", App.Version);

        TaskDialog dialog = new() {
            XamlRoot = App.XamlRoot,
            Title = "About",
            Content = new MarkdownViewer {
                Markdown = contents
            },
            Buttons = [
                TaskDialogButton.OKButton
            ]
        };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    public static async Task OpenDocumentationWebsite()
    {
        try {
            Process.Start(new ProcessStartInfo("https://tkmm.org/docs/using-mods/") {
                UseShellExecute = true
            });
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while trying to open the documentation website.");
            await ErrorDialog.ShowAsync(ex);
        }
    }

    public static async Task CheckForUpdates(bool isUserInvoked, CancellationToken ct = default)
    {
        if (!OperatingSystem.IsWindows()) {
            if (!isUserInvoked) return;

            await ApplicationUpdatesHelper.ShowUnsupportedPlatformDialog();
        }

        try {
            if (await ApplicationUpdatesHelper.HasAvailableUpdates() is Release release) {
                if (!isUserInvoked) {
                    MessageDialogResult result = await MessageDialog.Show(
                        TkLocale.System_Popup_UpdateAvailable,
                        TkLocale.System_Popup_UpdateAvailable_Title, MessageDialogButtons.YesNo);

                    if (result is not MessageDialogResult.Yes) {
                        return;
                    }
                }

                await RequestUpdate(release, ct);
                return;
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex,
                "An error occured while checking for application updates.");
            await ErrorDialog.ShowAsync(ex);
        }

        await new ContentDialog {
            Title = "Check for updates result",
            Content = "Software up to date.",
            PrimaryButtonText = "OK"
        }.ShowAsync();
    }

    public static Task RequestUpdate(Release release, CancellationToken ct = default)
    {
        return Dispatcher.UIThread.InvokeAsync(
            () => RequestUpdateInternal(release, ct));
    }

    private static async Task RequestUpdateInternal(Release release, CancellationToken ct = default)
    {
        if (!OperatingSystem.IsWindows()) {
            return;
        }
        
        try {
            await ApplicationUpdatesHelper.PerformUpdates(release, ct);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex,
                "An error occured while updating the application.");
            await ErrorDialog.ShowAsync(ex);
            return;
        }
        
        ContentDialog dialog = new() {
            Title = Locale[TkLocale.System_Popup_UpdateComplete_Title],
            Content = Locale[TkLocale.System_Popup_UpdateComplete],
            PrimaryButtonText = Locale[TkLocale.Action_Yes],
            SecondaryButtonText = Locale[TkLocale.Action_No]
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }

        await ApplicationUpdatesHelper.Restart();
    }

    [RelayCommand]
    public static async Task CleanupTempFolder()
    {
        try {
            string tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".temp");

            if (!Directory.Exists(tempFolder)) {
                return;
            }

            Directory.Delete(tempFolder, recursive: true);
            Directory.CreateDirectory(tempFolder);

            App.Toast("The temporary folder was successfully deleted.",
                "Temporary Files Cleared", NotificationType.Success, TimeSpan.FromSeconds(3));
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while trying to cleanup the temp folder.");
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public static async Task SoftClose()
    {
        try {
            Config.Shared.Save();
            TKMM.ModManager.Save();
            Environment.Exit(0);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while saving the mod manager state.");

            object errorReportResult = await ErrorDialog.ShowAsync(ex,
                TaskDialogStandardResult.Close, TaskDialogStandardResult.Cancel);
            if (errorReportResult is TaskDialogStandardResult.Close) {
                Environment.Exit(-1);
            }
        }
    }
}