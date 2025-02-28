using Microsoft.Win32;

namespace Tkmm.Core.Helpers;

public static class RegistryHelper
{
    public static void CreateGameBananaWebProtocol(string name, string appPath)
    {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

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
    }

    public static void CreateFileAssociations(string name, string extension, string appPath)
    {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

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

        using RegistryKey fileAssociation = classes.CreateSubKey(extension);
        fileAssociation.SetValue(null, name, RegistryValueKind.String);
    }
}