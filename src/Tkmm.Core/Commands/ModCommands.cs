using Cocona;
using Tkmm.Core.Components;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Commands;

public class ModCommands
{
    private const string INSTALL = "Import a mod from a folder, tkcl file, gamebanana mod url, or gamebanana mod id";

    [Command("import", Aliases = ["add", "install"], Description = INSTALL)]
    public static async Task InstallMod([Argument] string path)
    {
        ProfileManager.Shared.Mods.Add(
            await Mod.FromPath(path)
        );

        ProfileManager.Shared.Apply();
    }

    [Command("list", Aliases = ["ls"], Description = "Print information about each installed mod")]
    public static Task ListMods()
    {
        foreach (var mod in ProfileManager.Shared.Mods) {
            Console.WriteLine($"""
                - {mod.Name} ({mod.Version}) by {mod.Author}
                """);
        }

        return Task.CompletedTask;
    }
}
