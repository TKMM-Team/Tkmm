using ConsoleAppFramework;
using Tkmm.Core;

namespace Tkmm.CLI.Commands;

[RegisterCommands("mods")]
public class ModCommands
{
    public async Task Install([Argument] string arg)
    {
        Console.WriteLine($"Installing {arg}");
        
        FileStream? fs = null;
        await TKMM.Install(arg, File.Exists(arg) ? fs = File.OpenRead(arg) : null);

        if (fs is not null) {
            await fs.DisposeAsync();
        }
    }
}