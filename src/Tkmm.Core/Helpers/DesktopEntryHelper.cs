#if !SWITCH
using System.Diagnostics;
using System.Text;

namespace Tkmm.Core.Helpers;

public static class DesktopEntryHelper
{
    private const string URI_HANDLER_DESKTOP_FILE_NAME = "tkmm-uri-handler.desktop";
    private const string URI_SCHEME = "tkmm";
    private const string DisplayName = "TKMM";

    private static readonly string[] ToolPaths = ["/usr/bin", "/bin", "/usr/local/bin"];

    public static void TryRegisterUriSchemeHandler()
    {
        if (ResolveLinuxExecutablePath() is { } linuxExecutablePath) {
            RegisterUriSchemeHandler(linuxExecutablePath);
        }
    }

    public static void RegisterUriSchemeHandler(string executablePath)
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
        var desktopEntry = BuildDesktopEntry(executablePath);
        var needsWrite = !File.Exists(desktopFilePath)
                         || File.ReadAllText(desktopFilePath) != desktopEntry;

        if (needsWrite) {
            File.WriteAllText(desktopFilePath, desktopEntry, Encoding.UTF8);
        }

        UpdateDesktopDatabase(applicationsDirectory);
        EnsureMimeAppsListEntry();
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

    private static string BuildDesktopEntry(string executablePath)
    {
        var escapedExecutablePath = executablePath.Replace("\"", "\\\"", StringComparison.Ordinal);

        return $"""
                [Desktop Entry]
                Version=1.0
                Type=Application
                Name={DisplayName}
                Comment=TotK Mod Manager
                Exec="{escapedExecutablePath}" %u
                TryExec="{escapedExecutablePath}"
                Icon=tkmm
                MimeType=x-scheme-handler/{URI_SCHEME};
                Categories=Utility;
                NoDisplay=true
                Terminal=false
                StartupNotify=false

                """;
    }

    private static void RegisterMimeAssociation()
    {
        TryRunCommand("xdg-settings", $"set default-url-scheme-handler {URI_SCHEME} {URI_HANDLER_DESKTOP_FILE_NAME}");
        TryRunCommand("xdg-mime", $"default {URI_HANDLER_DESKTOP_FILE_NAME} x-scheme-handler/{URI_SCHEME}");
        TryRunCommand("gio", $"mime x-scheme-handler/{URI_SCHEME} {URI_HANDLER_DESKTOP_FILE_NAME}");
    }

    private static void UpdateDesktopDatabase(string applicationsDirectory)
    {
        TryRunCommand("update-desktop-database", applicationsDirectory);
    }

    private static void EnsureMimeAppsListEntry()
    {
        var configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config");
        Directory.CreateDirectory(configDirectory);

        var mimeAppsPath = Path.Combine(configDirectory, "mimeapps.list");
        var association = $"x-scheme-handler/{URI_SCHEME}={URI_HANDLER_DESKTOP_FILE_NAME};";

        var lines = File.Exists(mimeAppsPath)
            ? File.ReadAllLines(mimeAppsPath).ToList()
            : [];

        SetMimeAppsAssociation(lines, "Default Applications", association);
        SetMimeAppsAssociation(lines, "Added Associations", association);
        File.WriteAllLines(mimeAppsPath, lines);
    }

    private static void SetMimeAppsAssociation(List<string> lines, string sectionName, string association)
    {
        var sectionHeader = $"[{sectionName}]";
        var sectionIndex = lines.FindIndex(line => line.Trim() == sectionHeader);

        if (sectionIndex < 0) {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1])) {
                lines.Add(string.Empty);
            }

            lines.Add(sectionHeader);
            lines.Add(association);
            return;
        }

        var key = association[..association.IndexOf('=')];
        var entryIndex = -1;

        for (var i = sectionIndex + 1; i < lines.Count; i++) {
            var line = lines[i].Trim();

            if (line.StartsWith('[')) {
                break;
            }

            if (line.StartsWith(key, StringComparison.Ordinal)) {
                entryIndex = i;
                break;
            }
        }

        if (entryIndex >= 0) {
            lines[entryIndex] = association;
            return;
        }

        lines.Insert(sectionIndex + 1, association);
    }

    private static bool TryRunCommand(string fileName, string arguments)
    {
        foreach (var toolPath in ResolveToolPaths(fileName)) {
            if (TryRunCommandAtPath(toolPath, arguments)) {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> ResolveToolPaths(string fileName)
    {
        yield return fileName;

        foreach (var directory in ToolPaths) {
            yield return Path.Combine(directory, fileName);
        }
    }

    private static bool TryRunCommandAtPath(string filePath, string arguments)
    {
        try {
            using var process = Process.Start(new ProcessStartInfo {
                FileName = filePath,
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
