using Microsoft.Win32;

namespace Tkmm.Core.Helpers;

public static class RegistryHelper
{
    public static void CreateGameBananaWebProtocol(string name, string appPath)
    {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        using var software = Registry.CurrentUser.OpenSubKey("SOFTWARE", RegistryKeyPermissionCheck.ReadWriteSubTree)
                             ?? throw new InvalidOperationException("'CURRENT_USER/SOFTWARE' not found in registry");
        using var classes = software.OpenSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree)
                            ?? throw new InvalidOperationException("'CURRENT_USER/SOFTWARE/Classes' not found in registry");
        using var root = classes.CreateSubKey(name);
        root.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
        using var shell = root.CreateSubKey("shell");
        using var open = shell.CreateSubKey("open");
        using var command = open.CreateSubKey("command");
        command.SetValue(null, $"\"{appPath}\" \"%1\"", RegistryValueKind.String);
    }

    public static void CreateFileAssociations(string name, string extension, string appPath)
    {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        using var software = Registry.CurrentUser.OpenSubKey("SOFTWARE", RegistryKeyPermissionCheck.ReadWriteSubTree)
                             ?? throw new InvalidOperationException("'CURRENT_USER/SOFTWARE' not found in registry");
        using var classes = software.OpenSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree)
                            ?? throw new InvalidOperationException("'CURRENT_USER/SOFTWARE/Classes' not found in registry");
        using var root = classes.CreateSubKey(name);
        root.SetValue("URL Protocol", string.Empty, RegistryValueKind.String);
        using var shell = root.CreateSubKey("shell");
        using var open = shell.CreateSubKey("open");
        using var command = open.CreateSubKey("command");
        command.SetValue(null, $"\"{appPath}\" \"%1\"", RegistryValueKind.String);

        using var fileAssociation = classes.CreateSubKey(extension);
        fileAssociation.SetValue(null, name, RegistryValueKind.String);
    }
}