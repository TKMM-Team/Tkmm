using Tkmm.Core.Generics;
using Tkmm.Core.Services;
using TKMM.SarcTool.Core;

namespace Tkmm.Core.Components.Mergers;

public class SarcMergerShell : IMerger
{
    public Task Merge(IModItem[] mods, string output)
    {
        SarcMerger merger = new(mods.Select(x => x.SourceFolder), Path.Combine(output, "romfs"));
        return merger.MergeAsync();
    }
}
