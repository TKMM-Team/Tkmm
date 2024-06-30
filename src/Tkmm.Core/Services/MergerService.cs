using Tkmm.Core.Components;
using Tkmm.Core.Components.Mergers;
using Tkmm.Core.Components.Mergers.Special;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Services;

public class MergerService
{
    private static readonly IMerger[] _mergers = [
        new ContentMerger(),
        new ExefsMerger(),
        new MalsMergerShell(),
        new RsdbMergerShell(),
        new SarcMergerShell()
    ];

    public static async Task Merge()
    {
        Config.Shared.EnsureMergeOutput();
        await Merge(ProfileManager.Shared.Current, Config.Shared.MergeOutput);
    }

    public static async Task Merge(Profile profile)
    {
        Config.Shared.EnsureMergeOutput();
        await Merge(profile, Config.Shared.MergeOutput);
    }

    public static async Task Merge(string output) => await Merge(ProfileManager.Shared.Current, output);
    public static async Task Merge(Profile profile, string output)
    {
        IModItem[] mods = profile.Mods
            .Where(x => x.IsEnabled && x.Mod is not null)
            .Reverse()
            .SelectMany(x => x.Mod!.SelectModRecursive())
            .ToArray();

        if (mods.Length <= 0) {
            AppStatus.Set("Nothing to Merge", "fa-solid fa-code-merge",
                isWorkingStatus: false, temporaryStatusTime: 1.5,
                logLevel: LogLevel.Info);

            return;
        }

        if (Directory.Exists(output)) {
            AppStatus.Set($"Clearing output", "fa-solid fa-code-merge");
            DirectoryOperations.DeleteTargets(output, [TotkConfig.ROMFS, TotkConfig.EXEFS], recursive: true);
        }

        TriviaService.Start();
        await Task.Run(async () => {
            Directory.CreateDirectory(output);
            await MergeAsync(mods, output);
        });

        TriviaService.Stop();
        AppStatus.Set("Merge completed successfully", "fa-solid fa-list-check",
            isWorkingStatus: false, temporaryStatusTime: 1.5,
            logLevel: LogLevel.Info);
    }

    private static async Task MergeAsync(IModItem[] mods, string output)
    {
        Task[] tasks = new Task[_mergers.Length];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = _mergers[i].Merge(mods, output);
        }

        await Task.WhenAll(tasks);
        await RstbMergerShell.Shared.Merge(mods, output);
    }
}
