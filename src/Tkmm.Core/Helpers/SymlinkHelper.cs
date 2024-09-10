using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace Tkmm.Core.Helpers;

internal static class SymlinkHelper
{
    public static bool CreateMany((string Path, string PathToTarget)[] targets)
    {
        if (!OperatingSystem.IsWindows() || IsDeveloperModeEnabled()) {
            return CreateManyWithPermission(targets);
        }

        return CreateManyRequestPermission(targets);
    }

    private static bool CreateManyWithPermission((string Path, string PathToTarget)[] targets)
    {
        foreach ((string path, string pathToTarget)  in targets) {
            if (!TryDeleteFolder(path)) {
                continue;
            }

            Directory.CreateSymbolicLink(path, pathToTarget);
        }

        return true;
    }

    private static bool CreateManyRequestPermission((string Path, string PathToTarget)[] targets)
    {
        StringBuilder arguments = new();

        foreach ((string path, string pathToTarget)  in targets) {
            if (!TryDeleteFolder(path)) {
                continue;
            }

            arguments.Append($"""
                MKLINK /D "{path}" "{pathToTarget}" &&
                """);
        }

        ProcessStartInfo processInfo = new() {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"""
                /c {arguments}ECHO Success
                """,
            UseShellExecute = true,
            Verb = "runas"
        };

        try {
            Process.Start(processInfo)?.WaitForExit();
            return true;
        }
        catch {
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool IsDeveloperModeEnabled()
    {
        RegistryKey? key = Registry.LocalMachine.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock", writable: false);
        return (key?.GetValue("AllowDevelopmentWithoutDevLicense", 0) as int?) == 1;
    }

    private static bool TryDeleteFolder(string path)
    {
        if (!Directory.Exists(path)) {
            return true;
        }

        try {
            Directory.Delete(path);
        }
        catch (Exception ex) {
            AppLog.Log(ex);
            return false;
        }

        return true;
    }
}
