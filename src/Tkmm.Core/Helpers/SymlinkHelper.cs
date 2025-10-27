using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using TkSharp.Core;

namespace Tkmm.Core.Helpers;

public class SymlinkHelper
{
    public static async ValueTask CreateMany(IEnumerable<string> targets, string linkToPath)
    {
        if (OperatingSystem.IsWindows() && !IsDeveloperModeEnabled()) {
            await CreateManyRequestPermission(targets, linkToPath);
            return;
        }

        Create(targets, linkToPath);
    }

    private static void Create(IEnumerable<string> targets, string linkToPath)
    {
        foreach (string path in targets) {
            if (CheckFolder(path)) {
                Directory.CreateSymbolicLink(path, linkToPath);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private static async ValueTask CreateManyRequestPermission(IEnumerable<string> targets, string linkToPath)
    {
        StringBuilder arguments = new();

        foreach (string path in targets) {
            if (CheckFolder(path)) {
                arguments.Append($"""
                    MKLINK /D "{path}" "{linkToPath}" &&
                    """);
            }
        }

        ProcessStartInfo processInfo = new() {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/c {arguments}ECHO Success",
            UseShellExecute = true,
            Verb = "runas"
        };

        if (Process.Start(processInfo) is { } process) {
            await process.WaitForExitAsync();
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool IsDeveloperModeEnabled()
    {
        var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock", writable: false);
        return key?.GetValue("AllowDevelopmentWithoutDevLicense", 0) as int? == 1;
    }

    private static bool CheckFolder(string path)
    {
        if (!Directory.Exists(path) && Path.GetDirectoryName(path) is { } parent) {
            Directory.CreateDirectory(parent);
            return true;
        }

        if (Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any()) {
            TkLog.Instance.LogWarning(
                "The export location '{ExportLocationPath}' cannot be created because it has contents.",
                path);
            return false;
        }
        
        Directory.Delete(path, recursive: true);
        return true;
    }
}