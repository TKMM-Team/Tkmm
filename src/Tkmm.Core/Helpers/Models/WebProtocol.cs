using Microsoft.Win32;
using System.Runtime.Versioning;

namespace Tkmm.Core.Helpers.Models;

public class WebProtocol
{
    public static void Create(string name, string appPath)
    {
        if (OperatingSystem.IsWindows()) {
            CreateWindows(name, appPath);
        }
    }

    [SupportedOSPlatform("windows")]
    public static void CreateWindows(string name, string appPath)
    {
        using RegistryKey root = Registry.ClassesRoot.CreateSubKey(name);
        root.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
        using RegistryKey shell = root.CreateSubKey("shell");
        using RegistryKey open = shell.CreateSubKey("open");
        using RegistryKey command = open.CreateSubKey("command");
        command.SetValue(null, $"\"{appPath}\" %1", RegistryValueKind.String);
    }
}
