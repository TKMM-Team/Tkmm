#if SWITCH
using Avalonia.Controls;
#else
using System.IO.Compression;
using Humanizer;
using Tkmm.Core;
#endif
using System.Diagnostics;
using System.Runtime.InteropServices;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Octokit;
using Tkmm.Core.Helpers;
using Tkmm.Dialogs;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana.Helpers;
using TkSharp.Extensions.GameBanana.Models;

namespace Tkmm.Components;

public static class AppUpdater
{
#if !SWITCH
    private static readonly string RuntimeId = OperatingSystem.IsWindows() ? "win" : OperatingSystem.IsLinux() ? "linux" : "osx";
    private static readonly string AssetName = $"Tkmm-{RuntimeId}-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}.zip";
#endif

    public static async ValueTask CheckForUpdates(bool isUserInvoked, CancellationToken ct = default)
    {
#if NO_UPDATE
        if (isUserInvoked) {
            await MessageDialog.Show(
                TkLocale.System_Popup_UpdateNotSupported,
                TkLocale.System_Popup_UpdateNotSupported_Title);
        }

        return;
#endif

        if (await HasAvailableUpdates() is not { } release) {
            if (isUserInvoked) {
                await MessageDialog.Show(
                    TkLocale.System_Popup_SoftwareUpToDate,
                    TkLocale.System_Popup_Updater_Title);
            }

            return;
        }

#if !SWITCH
        var result = await MessageDialog.Show(
            TkLocale.System_Popup_UpdateAvailable,
            TkLocale.System_Popup_UpdateAvailable_Title, MessageDialogButtons.YesNo);
#else
        var result = await MessageDialog.Show(
            TkLocale.System_Popup_NxUpdateAvailable,
            TkLocale.System_Popup_UpdateAvailable_Title, MessageDialogButtons.YesNo);
#endif
        if (result is not MessageDialogResult.Yes) {
            return;
        }

#if !SWITCH
    Retry:
        TaskDialog taskDialog = new() {
            Header = Locale[TkLocale.System_Popup_Updater_Title],
            SubHeader = Locale[TkLocale.System_Popup_Updater],
            IconSource = new SymbolIconSource {
                Symbol = Symbol.Download
            },
            ShowProgressBar = true,
            XamlRoot = App.XamlRoot,
        };

        taskDialog.Opened += async (dialog, _) => {
            DownloadHelper.Reporters.Push(new DownloadReporter {
                ProgressReporter = new Progress<double>(
                    progress => dialog.SetProgressBarState(progress * 100.0, TaskDialogProgressState.Normal)
                ),
                SpeedReporter = new Progress<double>(_ => { })
            });

            try {
                await PerformUpdate(release, ct);
                dialog.Hide(TaskDialogStandardResult.Yes);
            }
            catch (Exception ex) {
                dialog.Header = Locale[TkLocale.System_Popup_UpdaterFailed_Title];
                dialog.SubHeader = ex.GetType().ToString().Humanize(LetterCasing.Title);
                dialog.ShowProgressBar = false;
                dialog.Buttons.Add(TaskDialogButton.RetryButton);
                dialog.Buttons.Add(TaskDialogButton.CancelButton);
                
                TkLog.Instance.LogError(ex, "Update failed.");
            }
            finally {
                DownloadHelper.Reporters.Pop();
            }
        };

        switch (await taskDialog.ShowAsync()) {
            case TaskDialogStandardResult.Retry:
                goto Retry;
            case TaskDialogStandardResult.Yes:
                Restart();
                return;
        }
#else
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
#endif
    }

    private static async ValueTask<Release?> HasAvailableUpdates()
    {
#if SWITCH
        var latest = await OctokitHelper.GetLatestRelease("TKMM-Team", "TKMM-NX");
        var latestCommit = latest.TargetCommitish;
        var currentCommit = await GetCurrentNxCommit();
        return latestCommit != currentCommit ? latest : null;
#else
        var latest = await OctokitHelper.GetLatestRelease("TKMM-Team", "Tkmm");
        return latest.TagName.Length < 1 || latest.TagName[1..] != App.Version ? latest : null;
#endif
    }

    private static async ValueTask PerformUpdate(Release release, CancellationToken ct = default)
    {
#if SWITCH
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
#else
        await using var stream = await OctokitHelper.DownloadReleaseAsset(release, AssetName, "Tkmm", ct);

        if (stream is null) {
            throw new Exception(
                $"Update failed: Could not locate and/or download release assets from '{release.TagName}'.");
        }

        Config.SaveAll();
        TKMM.ModManager.Save();

        ZipArchive archive = new(stream, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries) {
            string target = Path.Combine(AppContext.BaseDirectory, entry.FullName);
            if (File.Exists(target)) File.Move(target, $"{target}.moldy");
        }

        archive.ExtractToDirectory(AppContext.BaseDirectory);
#endif

        Restart();
    }

#if SWITCH
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
#endif

#if SWITCH
    private static async Task<string> GetCurrentNxCommit()
    {
        if (File.Exists("/etc/os-release")) {
            var lines = await File.ReadAllLinesAsync("/etc/os-release");

            foreach (var line in lines) {
                if (line.StartsWith("BUILD_ID=")) {
                    return line["BUILD_ID=".Length..].Trim('"');
                }
            }
        }
        return string.Empty;
    }
#endif

    private static void Restart()
    {
#if !SWITCH
        var processName = Path.GetFileName(Environment.ProcessPath) ?? string.Empty;

        switch (processName.Length) {
            case >= 6 when Path.GetExtension(processName.AsSpan()) is ".moldy":
                processName = processName[..^6];
                break;
            case 0:
                processName = OperatingSystem.IsWindows() ? "Tkmm.exe" : "Tkmm";
                break;
        }

        ProcessStartInfo processStart = new(processName) {
            UseShellExecute = true,
            WorkingDirectory = AppContext.BaseDirectory,
        };

        Process.Start(processStart);
        Environment.Exit(0);
#else
        Process.Start(new ProcessStartInfo {
            FileName = "sh",
            Arguments = "-c \"echo 'self' > /sys/devices/r2p/action && sh /usr/bin/tkmm-reboot\"",
            UseShellExecute = true,
        });
#endif
    }

    public static void CleanupUpdate()
    {
        foreach (var oldFile in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.moldy")) {
        Retry:
            try {
                File.Delete(oldFile);
            }
            catch {
                goto Retry;
            }
        }
    }
}