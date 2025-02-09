using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.Versioning;
using FluentAvalonia.UI.Controls;
using Octokit;
using Tkmm.Actions;
using Tkmm.Core.Helpers;

namespace Tkmm.Helpers;

public static class ApplicationUpdatesHelper
{
    public static async ValueTask<Release?> HasAvailableUpdates()
    {
        Release latest = await OctokitHelper.GetLatestRelease("TKMM-Team", "Tkmm");
        return latest.TagName != App.Version ? latest : null;
    }

    [SupportedOSPlatform("Windows")]
    public static async ValueTask PerformUpdates(Release release, CancellationToken ct = default)
    {
        await using Stream? stream = await OctokitHelper.DownloadReleaseAsset(release);

        if (stream is null) {
            throw new Exception(
                $"Update failed: Could not locate and/or download release assets from '{release.TagName}'.");
        }

        ZipArchive archive = new(stream, ZipArchiveMode.Read);
        foreach (ZipArchiveEntry entry in archive.Entries) {
            string target = Path.Combine(AppContext.BaseDirectory, entry.FullName);
            File.Move(target, $"{target}.moldy");
        }

        archive.ExtractToDirectory(AppContext.BaseDirectory);
    }

    public static async ValueTask ShowUnsupportedPlatformDialog()
    {
        await new ContentDialog {
            Title = "Unsupported operating system",
            Content = "In-app updates are only supported on Windows. Use the app store or your package manager to update.",
            PrimaryButtonText = "OK"
        }.ShowAsync();
    }

    public static async ValueTask Restart()
    {
        string processName = Path.GetFileName(Environment.ProcessPath) ?? string.Empty;

        switch (processName.Length) {
            case >= 6 when Path.GetExtension(processName.AsSpan()) is ".moldy":
                processName = processName[..6];
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
        await SystemActions.SoftClose();
    }

    public static async ValueTask CleanupUpdate()
    {
        await Task.Delay(2000);

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