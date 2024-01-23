using Cocona;

namespace Tkmm.Core.Commands;

public class GeneralCommands
{
    [Command("merge", Description = "Merge the mods into an output folder")]
    public void Merge([Option("output", ['o'])] string? output)
    {
        throw new NotImplementedException();
    }
}
