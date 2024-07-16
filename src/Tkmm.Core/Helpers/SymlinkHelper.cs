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
        foreach (var (path, pathToTarget)  in targets) {
            if (Directory.Exists(path)) {
                Directory.Delete(path);
            }

            Directory.CreateSymbolicLink(path, pathToTarget);
        }

        return true;
    }

    private static bool CreateManyRequestPermission((string Path, string PathToTarget)[] targets)
    {
        StringBuilder arguments = new();

        for (int i = 0; i < targets.Length;) {
            (string path, string pathToTarget) = targets[i];

            if (Directory.Exists(path)) {
                Directory.Delete(path);
            }

            arguments.Append($"""
                MKLINK /D "{path}" "{pathToTarget}" {(++i < targets.Length ? "&& " : string.Empty)}
                """);
        }

        ProcessStartInfo processInfo = new() {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"""
                /c {arguments}
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
}
