using Tkmm.Core.Components;
using Tkmm.Core.Components.Mergers;
using Tkmm.Core.Components.Mergers.Special;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Services;

public class MergerService
{
    private static readonly IMerger[] _mergers = [
        new ContentMerger(),
        new MalsMergerShell(),
        new RsdbMergerShell(),
        new SarcMergerShell()
    ];

    public static async Task Merge() => await Merge(ProfileManager.Shared.Current, Config.Shared.MergeOutput);
    public static async Task Merge(string output) => await Merge(ProfileManager.Shared.Current, output);
    public static async Task Merge(Profile profile) => await Merge(profile, Config.Shared.MergeOutput);
    public static async Task Merge(Profile profile, string output)
    {
        Mod[] mods = profile.Mods
            .Where(x => x.IsEnabled && x.Mod is not null)
            .Select(x => x.Mod!)
            .Reverse()
            .ToArray();

        if (mods.Length <= 0) {
            AppStatus.Set("Nothing to Merge", "fa-solid fa-code-merge",
                isWorkingStatus: false, temporaryStatusTime: 1.5,
                logLevel: LogLevel.Info);

            return;
        }

        // Define a list of strings for status messages
        var statusMessages = new List<string>
        {
            "HGStone Is Adding More Bacon...",
            "Mind Is Partying With The Bokoblins...",
            "Collin's headbutt is Super Effective! Need Xray!",
            "Vintii Broke The Master Sword...",
            "5th Is Watching...",
            "Bubbles is ensuring all bunnies are accounted for...",
            "Running From Gloom Hands...",
            "Dancing With Ganondorf (suavamente)...",
            "Zelda is looming starward...",
            "Echo is ranting about spear mult...",
            "Updating the Purah Pad...",
            "Roasting Koroks...",
            "Cleaning up Malice...",
            "Grinding for Zonaite...",
            "Eating Dubious Food...",
            "Taming Lynels"
        };

        // Create a random object for selecting a random string
        var random = new Random();

        // Start the merge process in a separate task
        var mergeTask = Task.Run(async () => {
            if (Directory.Exists(output)) {
                AppStatus.Set($"Clearing output", "fa-solid fa-code-merge");
                Directory.Delete(output, true);
            }

            Directory.CreateDirectory(output);
            await MergeAsync(mods, output);
        });

        _ = Task.Run(async () => {
            while (!mergeTask.IsCompleted) {
                // Randomly select a string from the list each time
                var randomMessage = statusMessages[random.Next(statusMessages.Count)];
                AppStatus.Set($"{randomMessage}", "fa-solid fa-code-merge", logLevel: LogLevel.None);
                await Task.Delay(5000);
            }
        });

        await mergeTask;

        AppStatus.Set("Merge completed successfully", "fa-solid fa-list-check",
            isWorkingStatus: false, temporaryStatusTime: 1.5,
            logLevel: LogLevel.Info);
    }



    private static async Task MergeAsync(Mod[] mods, string output)
    {
        Task[] tasks = new Task[_mergers.Length];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = _mergers[i].Merge(mods, output);
        }

        await Task.WhenAll(tasks);
        await RstbMergerShell.Shared.Merge(mods, output);
    }
}
