using Cocona;
using Tkmm.Core.Components;

namespace Tkmm.Core.Commands;

[HasSubCommands(typeof(Mods), Description = "Mods commands")]
public class GeneralCommands
{
    [Command("merge", Description = "Merge the mods into an output folder")]
    public void Merge([Option("output", ['o'])] string? output)
    {
        throw new NotImplementedException();
    }

    public class Mods
    {
        [Command("list", Description = "Print information about each installed mod")]
        public static void ListMods()
        {
            foreach (var mod in ModManager.Shared.Mods) {
                Console.WriteLine($"""
                    - {mod.Name} ({mod.Version}) by {mod.Author}
                    """);
            }
        }
    }
}
