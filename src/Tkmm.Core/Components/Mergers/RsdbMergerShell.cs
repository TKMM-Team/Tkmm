﻿using RsdbMerger.Core.Services;
using Tkmm.Core.Generics;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.Mergers;

public class RsdbMergerShell : IMerger
{
    public async Task Merge(IModItem[] mods, string output)
    {
        RsdbMergerService merger = new([.. mods.Select(x => Path.Combine(x.SourceFolder, TotkConfig.ROMFS))], Path.Combine(output, TotkConfig.ROMFS));
        await merger.MergeAsync();
    }
}
