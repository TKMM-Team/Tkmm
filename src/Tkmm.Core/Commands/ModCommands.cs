using Cocona;
using Tkmm.Core.Components;

namespace Tkmm.Core.Commands;

public class ModCommands
{
    private const string INSTALL = "Import a mod from a folder or tkcl file";
    private const string INSTALL_APPLY_OPTION = "Apply the changes to the mod list, only set to false if the app is currently running";

    [Command("import", Aliases = ["add", "install"], Description = INSTALL)]
    public static void InstallMod([Argument] string path, [Option(Description = INSTALL_APPLY_OPTION)] bool apply = true)
    {
        ModManager.Shared.Import(path);

        if (apply) {
            ModManager.Shared.Apply();
        }
    }

    [Command("list", Aliases = ["ls"], Description = "Print information about each installed mod")]
    public static Task ListMods()
    {
        foreach (var mod in ModManager.Shared.Mods) {
            Console.WriteLine($"""
                    - {mod.Name} ({mod.Version}) by {mod.Author}
                    """);
        }

        return Task.CompletedTask;
    }
}
