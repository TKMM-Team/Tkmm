using Cocona;
using Tkmm.Core.Components;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Services;

namespace Tkmm.Core.Commands;

public class ProfileCommands
{
    [Command("set-current", Aliases = ["set"], Description = "Sets the current profile")]
    public static Task SetCurrentProfile([Argument("The name of the profile to set")] string name)
    {
        if (ProfileManager.Shared.Profiles.FirstOrDefault(x => x.Name == name) is Profile profile) {
            Console.WriteLine($"Current profile set to '{name}'");
            ProfileManager.Shared.Current = profile;
            return Task.CompletedTask;
        }

        Console.WriteLine($"""
            The profile '{name}' does not exists.

            Use 'profile list' to list the profiles.
            """);
        return Task.CompletedTask;
    }

    [Command("list", Aliases = ["ls"], Description = "Print information about each created profile")]
    public static Task ListProfiles()
    {
        foreach (Profile profile in ProfileManager.Shared.Profiles) {
            Console.WriteLine(profile.Name);
            foreach (ProfileMod profileMod in profile.Mods) {
                Console.WriteLine($"""
                    - {profileMod.Mod?.Name} ({profileMod.Mod?.Version}) by {profileMod.Mod?.Author}
                    """);
            }
        }

        return Task.CompletedTask;
    }

    [Command("merge", Aliases = ["mg"], Description = "Merge the current profile")]
    public static async Task Merge([Option("output", ['o'], Description = "")] string? output = null)
    {
        await MergerService.Merge(output ?? Config.Shared.MergeOutput);
    }
}
