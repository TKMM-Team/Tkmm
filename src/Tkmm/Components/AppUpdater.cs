using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Humanizer;
using Tkmm.Core;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Octokit;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;
using Tkmm.Dialogs;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana.Helpers;
using TkSharp.Extensions.GameBanana.Models;

namespace Tkmm.Components;

public static class AppUpdater
{
#if !SWITCH
    private static readonly string RuntimeId = OperatingSystem.IsWindows() ? "win" : OperatingSystem.IsLinux() ? "linux" : "osx";

    private static string AssetName =>
        $"Tkmm-{RuntimeId}-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}.{(IsAppImage ? "AppImage" : "zip")}";

    private static bool IsAppImage => RuntimeId is "linux" && TryGetAppImagePath(out _);

    private static bool TryGetAppImagePath([NotNullWhen(true)] out string? appImagePath)
    {
        appImagePath = Environment.GetEnvironmentVariable("APPIMAGE");
        return appImagePath is not null
               && File.Exists(appImagePath)
               && Path.GetFileName(appImagePath).EndsWith(".AppImage", StringComparison.OrdinalIgnoreCase);
    }

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

        var result = await MessageDialog.Show(
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
    }

    private static async ValueTask<Release?> HasAvailableUpdates()
    {
        var latest = await OctokitHelper.GetLatestRelease("TKMM-Team", "Tkmm");
        return latest.TagName.Length < 1 || latest.TagName[1..] != App.Version ? latest : null;
    }

    private static async ValueTask PerformUpdate(Release release, CancellationToken ct = default)
    {
        await using var stream = await OctokitHelper.DownloadReleaseAsset(release, AssetName, "Tkmm", ct);

        if (stream is null) {
            throw new Exception(
                $"Update failed: Could not locate and/or download release assets from '{release.TagName}'.");
        }

        Config.SaveAll();
        TKMM.ModManager.Save();

        if (IsAppImage) {
            await UpdateAppImage(stream, ct);
            return;
        }

        EnsureProcessStartLoaded();

        ZipArchive archive = new(stream, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries) {
            var target = Path.Combine(AppContext.BaseDirectory, entry.FullName);
            if (File.Exists(target)) File.Move(target, $"{target}.moldy");
        }

        await archive.ExtractToDirectoryAsync(AppContext.BaseDirectory, ct);
        Restart();
    }

    private static async ValueTask UpdateAppImage(Stream stream, CancellationToken ct)
    {
        if (!TryGetAppImagePath(out var appImagePath)) {
            throw new InvalidOperationException("AppImage path could not be resolved.");
        }

        if (File.Exists(appImagePath)) {
            File.Move(appImagePath, $"{appImagePath}.moldy", overwrite: true);
        }

        await using (var output = File.Create(appImagePath)) {
            await stream.CopyToAsync(output, ct);
        }

        if (OperatingSystem.IsLinux()) {
            var mode = File.GetUnixFileMode(appImagePath);
            File.SetUnixFileMode(appImagePath,
                mode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
        }

        SingleInstanceAppManager.MarkRestarting();

        Process.Start(new ProcessStartInfo(appImagePath) {
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(appImagePath),
        });
        
        Environment.Exit(0);
    }

    private static void Restart()
    {
        var executableDirectory = AppContext.BaseDirectory;
        var processName = Path.GetFileName(Environment.ProcessPath) ?? string.Empty;

        if (processName.EndsWith(".moldy", StringComparison.Ordinal)) {
            processName = processName[..^6];
        }

        if (processName.Length == 0 || !Path.Exists(Path.Combine(executableDirectory, processName))) {
            processName = OperatingSystem.IsWindows() ? "Tkmm.exe" : "Tkmm";
        }

        SingleInstanceAppManager.MarkRestarting();

        ProcessStartInfo processStart = new(processName) {
            UseShellExecute = true,
            WorkingDirectory = executableDirectory,
        };

        Process.Start(processStart);
        Environment.Exit(0);
    }

    private static void EnsureProcessStartLoaded()
    {
        _ = Environment.ProcessId;
        RuntimeHelpers.PrepareMethod(
            typeof(Process).GetMethod(nameof(Process.Start), [typeof(ProcessStartInfo)])!.MethodHandle);
    }

#endif
    public static void CleanupUpdate()
    {
#if !SWITCH
        var cleanupDirectory = TryGetAppImagePath(out var appImagePath)
            ? Path.GetDirectoryName(appImagePath)
            : AppContext.BaseDirectory;

        if (string.IsNullOrEmpty(cleanupDirectory)) {
            return;
        }

        foreach (var oldFile in Directory.EnumerateFiles(cleanupDirectory, "*.moldy")) {
        Retry:
            try {
                File.Delete(oldFile);
            }
            catch {
                goto Retry;
            }
        }
#endif
    }
}