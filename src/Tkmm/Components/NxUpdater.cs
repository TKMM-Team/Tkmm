#if SWITCH
using Avalonia.Controls;
using System.Diagnostics;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Octokit;
using Tkmm.Core.Helpers;
using Tkmm.Dialogs;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana.Helpers;
using TkSharp.Extensions.GameBanana.Models;

namespace Tkmm.Components;

public static class NxUpdater
{
    public static async ValueTask CheckForUpdates(bool isUserInvoked, CancellationToken ct = default)
    {
        if (await HasAvailableUpdates() is not { } release) {
            if (isUserInvoked) {
                await MessageDialog.Show(
                    TkLocale.System_Popup_SoftwareUpToDate,
                    TkLocale.System_Popup_Updater_Title);
            }

            return;
        }

        var result = await MessageDialog.Show(
            TkLocale.System_Popup_NxUpdateAvailable,
            TkLocale.System_Popup_UpdateAvailable_Title, MessageDialogButtons.YesNo);
        
        if (result is not MessageDialogResult.Yes) {
            return;
        }

    Retry:
        var progressBar = new ProgressBar();
        var progressText = new TextBlock { Text = Locale[TkLocale.System_Popup_Updater] };
        var progressStack = new StackPanel {
            Children = { progressText, progressBar },
            Spacing = 10
        };

        var contentDialog = new ContentDialog {
            Title = Locale[TkLocale.System_Popup_Updater_Title],
            Content = progressStack,
            IsPrimaryButtonEnabled = false
        };

        _ = PerformUpdateAsync(release, CancellationToken.None, progressBar, progressText, contentDialog);

        var dialogResult = await contentDialog.ShowAsync();
        
        if (dialogResult == ContentDialogResult.Primary && contentDialog.PrimaryButtonText == "Retry") {
            goto Retry;
        }
        Restart();
    }

    private static async ValueTask<Release?> HasAvailableUpdates()
    {
        var latest = await OctokitHelper.GetLatestRelease("TKMM-Team", "TKMM-NX");
        var latestCommit = latest.TargetCommitish;
        var currentCommit = await GetCurrentNxCommit();
        return latestCommit != currentCommit ? latest : null;
    }

    private static async ValueTask PerformUpdate(Release release, CancellationToken ct = default)
    {
        await using var systemStream = await OctokitHelper.DownloadReleaseAsset(release, "update.tar", "TKMM-NX", ct);
        
        if (systemStream is null) {
            throw new Exception(
                $"Update failed: Could not locate and/or download update from '{release.TagName}'.");
        }

        var updateDir = "/storage/.update";
        var updatePath = Path.Combine(updateDir, "update.tar");
        
        Directory.CreateDirectory(updateDir);
        
        await using var updateFile = File.Create(updatePath);
        await systemStream.CopyToAsync(updateFile, ct);
    }

    private static async Task PerformUpdateAsync(Release release, CancellationToken ct, ProgressBar progressBar, TextBlock progressText, ContentDialog contentDialog)
    {
        var progressReporter = new Progress<double>(progress => {
            progressBar.Value = progress * 100.0;
            progressText.Text = $"{Locale[TkLocale.System_Popup_Updater]} ({(int)(progress * 100)}%)";
        });

        DownloadHelper.Reporters.Push(new DownloadReporter {
            ProgressReporter = progressReporter,
            SpeedReporter = new Progress<double>(_ => { })
        });

        try {
            await PerformUpdate(release, ct);
            contentDialog.Hide(ContentDialogResult.None);
        }
        catch (OperationCanceledException) {
            contentDialog.Hide(ContentDialogResult.None);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Update failed");
        }
        finally {
            DownloadHelper.Reporters.Pop();
        }
    }

    private static async Task<string> GetCurrentNxCommit()
    {
        if (!File.Exists("/etc/os-release")) {
            return string.Empty;
        }
        
        var lines = await File.ReadAllLinesAsync("/etc/os-release");

        foreach (var line in lines) {
            if (line.StartsWith("BUILD_ID=")) {
                return line["BUILD_ID=".Length..].Trim('"');
            }
        }
        return string.Empty;
    }

    private static void Restart()
    {
        Process.Start(new ProcessStartInfo {
            FileName = "sh",
            Arguments = "-c \"echo 'self' > /sys/devices/r2p/action && sh /usr/bin/tkmm-reboot\"",
            UseShellExecute = true,
        });
    }
}
#endif