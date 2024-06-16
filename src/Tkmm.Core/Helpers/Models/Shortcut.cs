using NxEditor.Launcher.Helpers.Win32;

namespace Tkmm.Core.Helpers.Models;

public enum Location
{
    Desktop,
    Application
}

public class Shortcut(string name, Location location, string target, params string[] keywords)
{
    public string Name { get; set; } = name;
    public Location Location { get; set; } = location;
    public string Target { get; set; } = target;
    public string[] Keywords { get; set; } = keywords;

    public static void Create(string name, Location location, string target, params string[] keywords)
    {
        Shortcut shortcut = new(name, location, target, keywords);

        if (OperatingSystem.IsWindows()) {
            shortcut.CreateWin32();
        }
        else {
            shortcut.CreateUnix();
        }
    }

    public static void Remove(string name, Location location)
    {
        Shortcut shortcut = new(name, location, string.Empty);

        string? path = null;
        if (OperatingSystem.IsWindows() && shortcut.GetWin32Location() is string windowsDirectory) {
            path = Path.Combine(windowsDirectory, $"{name}.lnk");
        }
        else if (OperatingSystem.IsLinux() && shortcut.GetWin32Location() is string linuxDirectory) {
            path = Path.Combine(linuxDirectory, $"{name}.desktop");
        }

        if (path is not null && File.Exists(path)) {
            File.Delete(path);
        }
    }

    private void CreateWin32()
    {
        if (GetWin32Location() is string location) {
            IShellLink link = (IShellLink)new ShellLink();
            link.SetPath(Target);
            link.SetIconLocation(Target, 0);
            link.SetWorkingDirectory(Path.GetDirectoryName(Target) ?? location);

            IPersistFile file = (IPersistFile)link;
            file.Save(Path.Combine(location, $"{Name}.lnk"), false);
        }
    }

    private void CreateUnix()
    {
        if (GetLinuxLocation() is string location) {
            using StreamWriter writer = new(Path.Combine(location, $"{Name}.desktop"));
            writer.WriteLine("[Desktop Entry]");
            writer.WriteLine($"Icon={Target}");
            writer.WriteLine($"Keywords={string.Join(';', Keywords)}");
            writer.WriteLine($"Name={Name}");
            writer.WriteLine($"Exec={Target}");
            writer.WriteLine("StartupNotify=true");
            writer.WriteLine("Terminal=false");
            writer.WriteLine("Type=Application");
        }
    }

    private string? GetWin32Location()
    {
        if (Location == Location.Application) {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
        }
        else if (Location == Location.Desktop) {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        return null;
    }

    private string? GetLinuxLocation()
    {
        if (Location == Location.Application) {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "applications");
        }

        return null;
    }
}