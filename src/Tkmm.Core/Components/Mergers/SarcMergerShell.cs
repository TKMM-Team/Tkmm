using Tkmm.Core.Generics;
using Tkmm.Core.Services;
using TKMM.SarcTool.Core;

namespace Tkmm.Core.Components.Mergers;

public class SarcMergerShell : IMerger
{
    public Task Merge(IModItem[] mods, string output)
    {
        SarcMerger merger = new(mods.Select(x => Path.Combine(x.SourceFolder, "romfs")).Where(x => Directory.Exists(x)), Path.Combine(output, "romfs"));
        merger.Merge();
        return Task.CompletedTask;
    }
}
