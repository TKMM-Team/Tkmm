using MalsMerger.Core;
using Tkmm.Core.Generics;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.Mergers;

public class MalsMergerShell : IMerger
{
    public Task Merge(IModItem[] mods, string output)
    {
        Merger merger = new([.. mods.Select(x => Path.Combine(x.SourceFolder, TotkConfig.ROMFS))], Path.Combine(output, TotkConfig.ROMFS), Config.Shared.GameLanguage);
        merger.Merge();
        return Task.CompletedTask;
    }
}
