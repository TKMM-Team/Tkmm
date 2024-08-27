using Microsoft.Win32;
using System.Runtime.Versioning;

namespace Tkmm.Core.Helpers.Models;

public class WindowsFileAssociation
{
    public static void Create(string name, string extension, string appPath)
    {
        if (OperatingSystem.IsWindows()) {
            CreateWindows(name, appPath);
        }
    }

    [SupportedOSPlatform("windows")]
    public static void CreateWindows(string name, string extension, string appPath)
    {
        using RegistryKey software = Registry.CurrentUser.OpenSubKey("SOFTWARE", RegistryKeyPermissionCheck.ReadWriteSubTree)
            ?? throw new InvalidOperationException("'CURRENT_USER/SOFTWARE' not found in registry");
        using RegistryKey classes = software.OpenSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree)
            ?? throw new InvalidOperationException("'CURRENT_USER/SOFTWARE/Classes' not found in registry");
        using RegistryKey root = classes.CreateSubKey(name);
        root.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
        using RegistryKey shell = root.CreateSubKey("shell");
        using RegistryKey open = shell.CreateSubKey("open");
        using RegistryKey command = open.CreateSubKey("command");
        command.SetValue(null, $"\"{appPath}\" %1", RegistryValueKind.String);

        using RegistryKey fileAssosiation = classes.CreateSubKey(extension);
        fileAssosiation.SetValue(null, name, RegistryValueKind.String);
    }
}
