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

    public static async Task Merge() => await Merge(ProfileManager.Shared.Current);
    public static async Task Merge(Profile profile)
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

        AppStatus.Set($"Merging '{profile.Name}'", "fa-solid fa-code-merge");

        if (Directory.Exists(Config.Shared.MergeOutput)) {
            AppStatus.Set($"Clearing output", "fa-solid fa-code-merge");
            Directory.Delete(Config.Shared.MergeOutput, true);
        }

        Directory.CreateDirectory(Config.Shared.MergeOutput);
        await Task.Run(async () => {
            await MergeAsync(mods);
        });

        AppStatus.Set("Merge completed successfully", "fa-solid fa-list-check",
            isWorkingStatus: false, temporaryStatusTime: 1.5,
            logLevel: LogLevel.Info);
    }

    private static async Task MergeAsync(Mod[] mods)
    {
        Task[] tasks = new Task[_mergers.Length];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = _mergers[i].Merge(mods);
        }

        await Task.WhenAll(tasks);
        await RstbMergerShell.Shared.Merge(mods);
    }
}
