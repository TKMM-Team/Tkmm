#if !SWITCH
using System.Diagnostics;
using System.Text;

namespace Tkmm.Core.Helpers;

public static class DesktopEntryHelper
{
    private const string URI_HANDLER_DESKTOP_FILE_NAME = "tkmm-uri-handler.desktop";
    private const string URI_SCHEME = "tkmm";

    public static void RegisterUriSchemeHandler(string displayName, string executablePath)
    {
        if (!OperatingSystem.IsLinux() || string.IsNullOrWhiteSpace(executablePath)) {
            return;
        }

        var applicationsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "share",
            "applications");

        Directory.CreateDirectory(applicationsDirectory);

        var desktopFilePath = Path.Combine(applicationsDirectory, URI_HANDLER_DESKTOP_FILE_NAME);
        var desktopEntry = BuildDesktopEntry(displayName, executablePath);
        var needsRegistration = !File.Exists(desktopFilePath)
                                || File.ReadAllText(desktopFilePath) != desktopEntry;

        if (needsRegistration) {
            File.WriteAllText(desktopFilePath, desktopEntry, Encoding.UTF8);
            UpdateDesktopDatabase(applicationsDirectory);
        }

        RegisterMimeAssociation();
    }

    public static string? ResolveLinuxExecutablePath()
    {
        if (!OperatingSystem.IsLinux()) {
            return null;
        }

        var appImagePath = Environment.GetEnvironmentVariable("APPIMAGE");
        if (!string.IsNullOrWhiteSpace(appImagePath) && File.Exists(appImagePath)) {
            return appImagePath;
        }

        return Environment.ProcessPath;
    }

    private static string BuildDesktopEntry(string displayName, string executablePath)
    {
        var escapedExecutablePath = executablePath.Replace("\"", "\\\"", StringComparison.Ordinal);

        return $"""
                [Desktop Entry]
                Version=1.0
                Type=Application
                Name={displayName}
                Exec="{escapedExecutablePath}" %u
                MimeType=x-scheme-handler/{URI_SCHEME};
                NoDisplay=true
                Terminal=false
                StartupNotify=false

                """;
    }

    private static void RegisterMimeAssociation()
    {
        if (!TryRunCommand("xdg-mime", $"default {URI_HANDLER_DESKTOP_FILE_NAME} x-scheme-handler/{URI_SCHEME}")) {
            TryRunCommand("gio", $"mime x-scheme-handler/{URI_SCHEME} {URI_HANDLER_DESKTOP_FILE_NAME}");
        }
    }

    private static void UpdateDesktopDatabase(string applicationsDirectory)
    {
        TryRunCommand("update-desktop-database", applicationsDirectory);
    }

    private static bool TryRunCommand(string fileName, string arguments)
    {
        try {
            using var process = Process.Start(new ProcessStartInfo {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process is null) {
                return false;
            }

            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch {
            return false;
        }
    }
}
#endif
