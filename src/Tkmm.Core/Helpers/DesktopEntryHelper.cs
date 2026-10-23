#if !SWITCH
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TkSharp.Core;

namespace Tkmm.Core.Helpers;

public static class DesktopEntryHelper
{
    private const string DESKTOP_FILE_NAME = "tkmm-uri-handler.desktop";
    private const string MIME_PACKAGE_FILE_NAME = "tkmm-tkcl.xml";
    private const string URI_SCHEME = "tkmm";
    private const string PACKAGE_MIME_TYPE = "application/x-tkcl";
    private const string ICON_NAME = "tkmm";
    private const string MIME_ICON_NAME = "application-x-tkcl";
    private const string SCALABLE_APPS_DIR = "scalable/apps";
    private const string SCALABLE_MIME_DIR = "scalable/mimetypes";
    private const string DISPLAY_NAME = "TKMM";
    private const string STARTUP_WM_CLASS = "Tkmm";
    private const string ICON_RESOURCE_NAME = "Tkmm.Core.Resources.tkmm.svg";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly string[] ToolPaths = ["/usr/bin", "/bin", "/usr/local/bin"];
    private static readonly string[] ScalableDirectories = [SCALABLE_APPS_DIR, SCALABLE_MIME_DIR];

    public static void TryRegisterUriSchemeHandler()
    {
        if (ResolveLinuxExecutablePath() is { } linuxExecutablePath) {
            RegisterUriSchemeHandler(linuxExecutablePath);
        }
    }

    private static void RegisterUriSchemeHandler(string executablePath)
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

        var desktopFilePath = Path.Combine(applicationsDirectory, DESKTOP_FILE_NAME);
        File.WriteAllText(desktopFilePath, BuildDesktopEntry(executablePath), Utf8NoBom);

        InstallIcons();
        InstallPackageMimeType();
        UpdateDesktopDatabase(applicationsDirectory);
        EnsureMimeAppsListEntry();
        RegisterMimeAssociation();
    }

    private static string? ResolveLinuxExecutablePath()
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
        var quotedExecutablePath = EscapeQuotedDesktopExecPath(executablePath);

        return string.Join('\n', [
            "[Desktop Entry]",
            "Version=1.0",
            "Type=Application",
            $"Name={DISPLAY_NAME}",
            $"Exec=\"{quotedExecutablePath}\" %u",
            $"Icon={ICON_NAME}",
            $"StartupWMClass={STARTUP_WM_CLASS}",
            $"MimeType={PACKAGE_MIME_TYPE};x-scheme-handler/{URI_SCHEME};",
            "Terminal=false",
            string.Empty
        ]);
    }

    private static string EscapeQuotedDesktopExecPath(string path)
    {
        var builder = new StringBuilder(path.Length * 2);

        foreach (var ch in path) {
            if (ch is '"' or '\\') {
                builder.Append('\\');
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }

    private static void InstallPackageMimeType()
    {
        var mimeRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "share",
            "mime");
        var packagesDirectory = Path.Combine(mimeRoot, "packages");
        Directory.CreateDirectory(packagesDirectory);

        var packageXml = string.Join('\n', [
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
            "<mime-info xmlns=\"http://www.freedesktop.org/standards/shared-mime-info\">",
            $"  <mime-type type=\"{PACKAGE_MIME_TYPE}\">",
            "    <comment>TKMM Package</comment>",
            $"    <icon name=\"{MIME_ICON_NAME}\"/>",
            "    <glob pattern=\"*.tkcl\"/>",
            "    <glob pattern=\"*.TKCL\"/>",
            "  </mime-type>",
            "</mime-info>",
            string.Empty
        ]);

        File.WriteAllText(Path.Combine(packagesDirectory, MIME_PACKAGE_FILE_NAME), packageXml, Utf8NoBom);
        TryRunCommand("update-mime-database", mimeRoot);
    }

    private static void RegisterMimeAssociation()
    {
        TryRunCommand("xdg-settings", "set", "default-url-scheme-handler", URI_SCHEME, DESKTOP_FILE_NAME);
        TryRunCommand("xdg-mime", "default", DESKTOP_FILE_NAME, $"x-scheme-handler/{URI_SCHEME}");
        TryRunCommand("xdg-mime", "default", DESKTOP_FILE_NAME, PACKAGE_MIME_TYPE);
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

        string[] associations = [
            $"x-scheme-handler/{URI_SCHEME}={DESKTOP_FILE_NAME};",
            $"{PACKAGE_MIME_TYPE}={DESKTOP_FILE_NAME};"
        ];

        List<string> lines = File.Exists(mimeAppsPath) ? [.. File.ReadAllLines(mimeAppsPath)] : [];

        foreach (var association in associations) {
            SetMimeAppsAssociation(lines, "Default Applications", association);
            SetMimeAppsAssociation(lines, "Added Associations", association);
        }

        File.WriteAllText(mimeAppsPath, string.Join('\n', lines) + '\n', Utf8NoBom);
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

            if (!line.StartsWith(key, StringComparison.Ordinal)) {
                continue;
            }

            entryIndex = i;
            break;
        }

        if (entryIndex >= 0) {
            lines[entryIndex] = association;
            return;
        }

        lines.Insert(sectionIndex + 1, association);
    }

    private static void InstallIcons()
    {
        var svg = ReadEmbeddedIconSvg();
        if (svg is null) {
            return;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(home)) {
            return;
        }

        InstallSvgTheme(
            Path.Combine(home, ".local", "share", "icons", "hicolor"),
            "/usr/share/icons/hicolor/index.theme",
            svg);

        var theme = DetectIconTheme(home);
        if (!string.IsNullOrEmpty(theme) && !theme.Equals("hicolor", StringComparison.OrdinalIgnoreCase)) {
            var userTheme = Path.Combine(home, ".local", "share", "icons", theme);
            var systemIndex = $"/usr/share/icons/{theme}/index.theme";
            var indexSource = File.Exists(Path.Combine(userTheme, "index.theme"))
                ? Path.Combine(userTheme, "index.theme")
                : systemIndex;
            InstallSvgTheme(userTheme, indexSource, svg);
        }

        TryRunCommand("xdg-desktop-menu", "forceupdate");
        ClearThumbnails(home);
    }

    private static string? ReadEmbeddedIconSvg()
    {
        try {
            using var stream = typeof(DesktopEntryHelper).Assembly.GetManifestResourceStream(ICON_RESOURCE_NAME);
            if (stream is null) {
                return null;
            }

            using var reader = new StreamReader(stream, Utf8NoBom);
            return reader.ReadToEnd();
        }
        catch (Exception ex) {
            return null;
        }
    }

    private static void InstallSvgTheme(string themeRoot, string? indexSource, string svg)
    {
        Directory.CreateDirectory(themeRoot);
        var indexPath = Path.Combine(themeRoot, "index.theme");
        var isHicolor = themeRoot.Replace('\\', '/').EndsWith("/hicolor", StringComparison.OrdinalIgnoreCase);

        string baseText;
        if (isHicolor && File.Exists("/usr/share/icons/hicolor/index.theme")) {
            baseText = File.ReadAllText("/usr/share/icons/hicolor/index.theme");
        }
        else if (File.Exists(indexPath)) {
            baseText = File.ReadAllText(indexPath);
        }
        else if (!string.IsNullOrEmpty(indexSource) && File.Exists(indexSource)) {
            baseText = File.ReadAllText(indexSource);
        }
        else {
            baseText = "[Icon Theme]\nName=TKMM overlay\nInherits=hicolor\nDirectories=\n";
        }

        File.WriteAllText(indexPath, EnsureScalableDirectories(baseText), Utf8NoBom);

        var cache = Path.Combine(themeRoot, "icon-theme.cache");
        if (File.Exists(cache)) {
            try {
                File.Delete(cache);
            }
            catch (Exception ex) {
                TkLog.Instance.LogDebug(ex, "Failed to delete icon theme cache {Path}", cache);
            }
        }

        WriteSvg(Path.Combine(themeRoot, SCALABLE_APPS_DIR, $"{ICON_NAME}.svg"), svg);
        WriteSvg(Path.Combine(themeRoot, SCALABLE_MIME_DIR, $"{MIME_ICON_NAME}.svg"), svg);

        TryRunCommand("gtk-update-icon-cache", "-f", "-t", themeRoot);
        TryRunCommand("update-icon-caches", themeRoot);
    }

    private static void WriteSvg(string path, string svg)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, svg, Utf8NoBom);
    }

    private static string EnsureScalableDirectories(string indexText)
    {
        var dirLine = new Regex(
            @"^(?<pre>Directories\s*=\s*)(?<dirs>.*)$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        var match = dirLine.Match(indexText);
        if (match.Success) {
            var dirs = match.Groups["dirs"].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            foreach (var scalableDir in ScalableDirectories) {
                if (!dirs.Exists(dir => dir.Equals(scalableDir, StringComparison.OrdinalIgnoreCase))) {
                    dirs.Add(scalableDir);
                }
            }

            indexText = dirLine.Replace(indexText, match.Groups["pre"].Value + string.Join(',', dirs), 1);
        }
        else {
            indexText = indexText.TrimEnd() + "\nDirectories=" + string.Join(',', ScalableDirectories) + "\n";
        }

        foreach (var scalableDir in ScalableDirectories) {
            if (Regex.IsMatch(
                    indexText,
                    @"^\[" + Regex.Escape(scalableDir) + @"\]\s*$",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase)) {
                continue;
            }

            var context = scalableDir.Equals(SCALABLE_MIME_DIR, StringComparison.OrdinalIgnoreCase)
                ? "MimeTypes"
                : "Applications";

            indexText = indexText.TrimEnd() +
                        $"\n\n[{scalableDir}]\nSize=128\nContext={context}\nType=Scalable\nMinSize=1\nMaxSize=512\n";
        }

        return indexText;
    }

    private static string? DetectIconTheme(string home)
    {
        var fromGsettings = RunCapture("gsettings", "get", "org.gnome.desktop.interface", "icon-theme");
        if (!string.IsNullOrWhiteSpace(fromGsettings)) {
            var name = fromGsettings.Trim().Trim('\'', '"');
            if (name.Length > 0) {
                return name;
            }
        }

        var gtkIni = Path.Combine(home, ".config", "gtk-3.0", "settings.ini");
        
        if (!File.Exists(gtkIni)) {
            return Environment.GetEnvironmentVariable("GTK_THEME")?.Split(':', 2)[0].Trim();
        }
        
        foreach (var line in File.ReadLines(gtkIni)) {
            if (line.StartsWith("gtk-icon-theme-name=", StringComparison.OrdinalIgnoreCase)) {
                return line.Split('=', 2)[1].Trim();
            }
        }

        return Environment.GetEnvironmentVariable("GTK_THEME")?.Split(':', 2)[0].Trim();
    }

    private static void ClearThumbnails(string home)
    {
        var root = Path.Combine(home, ".cache", "thumbnails");
        if (!Directory.Exists(root)) {
            return;
        }

        foreach (var dir in Directory.EnumerateDirectories(root)) {
            foreach (var file in Directory.EnumerateFiles(dir, "*.png")) {
                try {
                    File.Delete(file);
                }
                catch (Exception ex) {
                    TkLog.Instance.LogDebug(ex, "Failed to delete thumbnail {Path}", file);
                }
            }
        }
    }

    private static void TryRunCommand(string fileName, params string[] arguments)
    {
        _ = RunCapture(fileName, arguments);
    }

    private static string? RunCapture(string fileName, params string[] arguments)
    {
        return ResolveToolPaths(fileName)
            .Select(toolPath => RunCaptureAtPath(toolPath, arguments))
            .OfType<string>().FirstOrDefault();
    }

    private static IEnumerable<string> ResolveToolPaths(string fileName)
    {
        yield return fileName;

        foreach (var directory in ToolPaths) {
            yield return Path.Combine(directory, fileName);
        }
    }

    private static string? RunCaptureAtPath(string filePath, params string[] arguments)
    {
        try {
            var start = new ProcessStartInfo {
                FileName = filePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            foreach (var argument in arguments) {
                start.ArgumentList.Add(argument);
            }

            using var process = Process.Start(start);
            if (process is null) {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return process.ExitCode == 0 ? output : null;
        }
        catch (Exception ex) {
            TkLog.Instance.LogDebug(ex, "Failed to run {Path}", filePath);
            return null;
        }
    }
}
#endif
