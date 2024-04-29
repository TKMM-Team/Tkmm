using Cocona;
using Tkmm.Core.Components;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Services;

namespace Tkmm.Core.Commands;

[HasSubCommands(typeof(ModCommands), "mods", Description = "Mod commands")]
public class GeneralCommands
{
    [Command("merge", Description = "Merge the mods from a profile into an output folder")]
    public static async Task Merge([Option("profile", ['p'])] string? profile, [Option("output", ['o'])] string? output)
    {
        Profile? target = null;
        if (profile is not null) {
            target = ProfileManager.Shared.Profiles.FirstOrDefault(x => x.Name == profile);
        }

        await MergerService.Merge(target ??= ProfileManager.Shared.Current, output ??= Config.Shared.MergeOutput);
    }
}
