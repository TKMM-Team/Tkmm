using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.Mergers.Special;

public class RstbMergerShell : IMerger
{
    private static readonly Lazy<RstbMergerShell> _shared = new(() => new());
    public static RstbMergerShell Shared => _shared.Value;

    public Task Merge(IModItem[] mods, string output)
    {
        return ToolHelper.Call(Tool.RestblMerger,
            "--action", "single-mod",
            "--use-checksums",
            "--version", TotkConfig.Shared.Version.ToString(),
            "--mod-path", output,
            "--compress"
        ).WaitForExitAsync();
    }
}
