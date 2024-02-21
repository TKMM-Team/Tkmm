using Tkmm.Core.Generics;
using Tkmm.Core.Services;
using TotkRstbGenerator.Core;

namespace Tkmm.Core.Components.Mergers.Special;

public class RstbMergerShell : IMerger
{
    private static readonly Lazy<RstbMergerShell> _shared = new(() => new());
    public static RstbMergerShell Shared => _shared.Value;

    public async Task Merge(IModItem[] mods, string output)
    {
        RstbGenerator generator = new(output);
        await generator.GenerateAsync();
    }
}
