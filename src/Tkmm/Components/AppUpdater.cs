using System.Diagnostics;
using System.IO.Compression;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Octokit;
using Tkmm.Core;
using Tkmm.Core.Helpers;
using Tkmm.Dialogs;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana.Helpers;
using TkSharp.Extensions.GameBanana.Models;

namespace Tkmm.Components;

public static class AppUpdater
{
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

        if (await HasAvailableUpdates() is not Release release) {
            if (isUserInvoked) {
                await MessageDialog.Show(
                    TkLocale.System_Popup_UpdateAvailable,
                    TkLocale.System_Popup_UpdateAvailable_Title);
            }

            return;
        }

        MessageDialogResult result = await MessageDialog.Show(
            TkLocale.System_Popup_UpdateAvailable,
            TkLocale.System_Popup_UpdateAvailable_Title, MessageDialogButtons.YesNo);

        if (result is not MessageDialogResult.Yes) {
            return;
        }

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

        taskDialog.Opened += async (dialog, e) => {
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
                TkLog.Instance.LogError(ex, "Update failed.");
                dialog.Header = Locale[TkLocale.System_Popup_UpdaterFailed_Title];
                dialog.SubHeader = ex.GetType().ToString().Humanize(LetterCasing.Title);
                dialog.ShowProgressBar = false;
                dialog.Buttons.Add(TaskDialogButton.RetryButton);
                dialog.Buttons.Add(TaskDialogButton.CancelButton);
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
    }

    private static async ValueTask<Release?> HasAvailableUpdates()
    {
        Release latest = await OctokitHelper.GetLatestRelease("TKMM-Team", "Tkmm");
        return latest.TagName.Length < 1 || latest.TagName[1..] != App.Version ? latest : null;
    }

    private static async ValueTask PerformUpdate(Release release, CancellationToken ct = default)
    {
        await using Stream? stream = await OctokitHelper.DownloadReleaseAsset(release, ct);

        if (stream is null) {
            throw new Exception(
                $"Update failed: Could not locate and/or download release assets from '{release.TagName}'.");
        }

        Config.SaveAll();
        TKMM.ModManager.Save();

        ZipArchive archive = new(stream, ZipArchiveMode.Read);
        foreach (ZipArchiveEntry entry in archive.Entries) {
            string target = Path.Combine(AppContext.BaseDirectory, entry.FullName);
            File.Move(target, $"{target}.moldy");
        }

        archive.ExtractToDirectory(AppContext.BaseDirectory);

        Restart();
    }

    private static void Restart()
    {
        string processName = Path.GetFileName(Environment.ProcessPath) ?? string.Empty;

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
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
        };

        Process.Start(processStart);
        Environment.Exit(0);
    }

    public static void CleanupUpdate()
    {
        foreach (string oldFile in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.moldy")) {
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