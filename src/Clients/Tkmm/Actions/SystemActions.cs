using System.Diagnostics;
using System.Runtime.Versioning;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Markdown.Avalonia.Full;
using Microsoft.Extensions.Logging;
using Octokit;
using Tkmm.Core;
using Tkmm.Dialogs;
using Tkmm.Helpers;

namespace Tkmm.Actions;

public sealed partial class SystemActions : GuardedActionGroup<SystemActions>
{
    protected override string ActionGroupName { get; } = nameof(SystemActions).Humanize();

    [RelayCommand]
    public async Task ShowAboutDialog()
    {
        await CanActionRun(showError: false);
        
        await using Stream aboutFileStream = AssetLoader.Open(new Uri("avares://Tkmm/Assets/About.md"));
        string contents = await new StreamReader(aboutFileStream).ReadToEndAsync();

        contents = contents.Replace("@@version@@", App.Version);

        Uri markdownStylePath = new("avares://Tkmm/Styles/Markdown.axaml");

        TaskDialog dialog = new() {
            XamlRoot = App.XamlRoot,
            Title = "About",
            Content = new MarkdownScrollViewer {
                Markdown = contents,
                Styles = {
                    new StyleInclude(markdownStylePath) {
                        Source = markdownStylePath
                    }
                }
            },
            Buttons = [
                TaskDialogButton.OKButton
            ]
        };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    public async Task OpenDocumentationWebsite()
    {
        await CanActionRun(showError: false);

        try {
            Process.Start(new ProcessStartInfo("https://tkmm.org/docs/using-mods/") {
                UseShellExecute = true
            });
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured while trying to open the documentation website.");
            await ErrorDialog.ShowAsync(ex);
        }
    }
    
    [RelayCommand]
    public async Task CheckForUpdates(CancellationToken ct = default)
    {
        await CanActionRun(showError: false);

        if (!OperatingSystem.IsWindows()) {
            await ApplicationUpdatesHelper.ShowUnsupportedPlatformDialog();
            return;
        }

        try {
            if (await ApplicationUpdatesHelper.HasAvailableUpdates() is Release release) {
                await RequestUpdate(release, ct);
                return;
            }
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex,
                "An error occured while checking for application udpates.");
            await ErrorDialog.ShowAsync(ex);
        }
        
        await new ContentDialog {
            Title = "Check for updates result",
            Content = "Software up to date.",
            PrimaryButtonText = "OK"
        }.ShowAsync();
    }

    public async Task RequestUpdate(Release release, CancellationToken ct = default)
    {
        await CanActionRun(showError: false);
        
        if (!OperatingSystem.IsWindows()) {
            await ApplicationUpdatesHelper.ShowUnsupportedPlatformDialog();
            return;
        }
        
        ContentDialog dialog = new() {
            Title = "Update available. Proceed with update?",
            Content = "Your current session will be saved and closed, are you sure you wish to proceed?",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "Cancel"
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }

        try {
            await ApplicationUpdatesHelper.PerformUpdates(release, ct);
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex,
                "An error occured while updating the application.");
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public async Task CleanupTempFolder()
    {
        await CanActionRun(showError: false);
        
        try {
            string tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".temp");
            
            if (!Directory.Exists(tempFolder)) {
                return;
            }

            Directory.Delete(tempFolder, recursive: true);
            Directory.CreateDirectory(tempFolder);
            
            App.Toast("The temporary folder was succesfully deleted.",
                "Temporary Files Cleared", NotificationType.Success, TimeSpan.FromSeconds(3));
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured while trying to cleanup the temp folder.");
            await ErrorDialog.ShowAsync(ex);
        }
    }

    [RelayCommand]
    public async Task SoftClose()
    {
        await CanActionRun(showError: false);
        
        try {
            Config.Shared.Save();
            await TKMM.ModManager.Save();
            await TKMM.ShopManager.Save();
            Environment.Exit(0);
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured while saving the mod manager state.");

            object errorReportResult = await ErrorDialog.ShowAsync(ex, TaskDialogButton.CloseButton, TaskDialogButton.CancelButton);
            if (Equals(errorReportResult, TaskDialogButton.CloseButton)) {
                Environment.Exit(-1);
            }
        }
    }
}