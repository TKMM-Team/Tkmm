using System.Diagnostics;
using Avalonia.Controls.Notifications;
using Avalonia.Platform;
using AvaMark;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Components;
using Tkmm.Core;
using Tkmm.Core.Logging;
using Tkmm.Dialogs;
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
        try {
            await AppUpdater.CheckForUpdates(isUserInvoked, ct);
        }
        catch (HttpRequestException ex) {
            string truncatedEx = ex.ToString().Split(Environment.NewLine)[0];
            TkLog.Instance.LogWarning("An error occured while checking for updates: {truncatedEx}", truncatedEx);
        }
        catch (Exception ex) {
            TkLog.Instance.LogWarning("An error occured while checking for updates: {ex}", ex);
        }

    }

    [RelayCommand]
    public static async Task CleanupTempFolder()
    {
        try {
            string tempFolder = Path.Combine(Path.GetTempPath(), "tkmm");

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
    public async Task OpenLogsFolder()
    {
        await CanActionRun(showError: false);

        try {
            ProcessStartInfo info = new() {
                FileName = DesktopLogger.LogsFolder,
                UseShellExecute = true,
                Verb = "open"
            };

            Process.Start(info);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "An error occured while opening the logs folder.");
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
            TkLog.Instance.LogError(ex, "An error occured while attempting to close the application.");

            object errorReportResult = await ErrorDialog.ShowAsync(ex,
                TaskDialogStandardResult.Close, TaskDialogStandardResult.Cancel);
            if (errorReportResult is TaskDialogStandardResult.Close) {
                Environment.Exit(-1);
            }
        }
    }
}