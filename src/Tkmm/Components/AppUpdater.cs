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

    public static bool IsAppImage => RuntimeId is "linux" && TryGetAppImagePath(out _);

    private static bool TryGetAppImagePath([NotNullWhen(true)] out string? appImagePath)
    {
        appImagePath = Environment.GetEnvironmentVariable("APPIMAGE");
        return appImagePath is not null
               && File.Exists(appImagePath)
               && Path.GetFileName(appImagePath).EndsWith(".AppImage", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetAppBundlePath([NotNullWhen(true)] out string? appBundlePath)
    {
        appBundlePath = null;
        if (!OperatingSystem.IsMacOS() || Path.GetDirectoryName(Environment.ProcessPath) is not { } macOsDir) {
            return false;
        }

        var bundleDir = Path.GetFullPath(Path.Combine(macOsDir, "..", ".."));
        if (bundleDir.EndsWith(".moldy", StringComparison.Ordinal)) {
            bundleDir = bundleDir[..^6];
        }

        if (!bundleDir.EndsWith(".app", StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        appBundlePath = bundleDir;
        return true;
    }

    private static bool TryGetReplaceTarget([NotNullWhen(true)] out string? targetPath)
        => TryGetAppImagePath(out targetPath) || TryGetAppBundlePath(out targetPath);

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

        if (TryGetReplaceTarget(out var targetPath)) {
            await ReplaceTarget(targetPath, stream, ct);
            Restart();
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

    private static async ValueTask ReplaceTarget(string targetPath, Stream stream, CancellationToken ct)
    {
        var moldy = $"{targetPath}.moldy";
        if (Directory.Exists(targetPath)) {
            var extractDir = Path.Combine(Path.GetTempPath(), "tkmm-update");
            if (Directory.Exists(extractDir)) Directory.Delete(extractDir, recursive: true);
            Directory.CreateDirectory(extractDir);

            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            await archive.ExtractToDirectoryAsync(extractDir, ct);

            var newApp = Directory.EnumerateDirectories(extractDir, "*.app").FirstOrDefault()
                         ?? throw new InvalidOperationException("The update archive does not contain a macOS app bundle.");

            if (Directory.Exists(moldy)) Directory.Delete(moldy, recursive: true);
            Directory.Move(targetPath, moldy);
            Directory.Move(newApp, targetPath);
            Directory.Delete(extractDir, recursive: true);
            return;
        }

        if (File.Exists(targetPath)) {
            File.Move(targetPath, moldy, overwrite: true);
        }

        await using (var output = File.Create(targetPath)) {
            await stream.CopyToAsync(output, ct);
        }

        if (OperatingSystem.IsLinux()) {
            var mode = File.GetUnixFileMode(targetPath);
            File.SetUnixFileMode(targetPath,
                mode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
        }
    }

    public static void Restart()
    {
        if (TryGetReplaceTarget(out var targetPath)) {
            SingleInstanceAppManager.MarkRestarting();
            Process.Start(Directory.Exists(targetPath)
                ? new ProcessStartInfo("open") { ArgumentList = { "-n", targetPath } }
                : new ProcessStartInfo(targetPath) {
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(targetPath)
                });
            Environment.Exit(0);
        }

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
        var cleanupDirectory = TryGetReplaceTarget(out var targetPath)
            ? Path.GetDirectoryName(targetPath)
            : AppContext.BaseDirectory;

        if (string.IsNullOrEmpty(cleanupDirectory)) {
            return;
        }

        foreach (var oldEntry in Directory.GetFileSystemEntries(cleanupDirectory, "*.moldy")) {
        Retry:
            try {
                if (Directory.Exists(oldEntry)) {
                    Directory.Delete(oldEntry, recursive: true);
                }
                else {
                    File.Delete(oldEntry);
                }
            }
            catch {
                goto Retry;
            }
        }
#endif
    }
}
