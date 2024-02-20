using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.Mergers;

public class RsdbMergerShell : IMerger
{
    public Task Merge(IModItem[] mods, string output)
    {
        return ToolHelper.Call(Tool.RsdbMerger,
            "--apply-changelogs", string.Join('|', mods.Select(x => x.SourceFolder)),
            "--output", Path.Combine(output, "romfs", "RSDB"),
            "--version", TotkConfig.Shared.Version.ToString()
        ).WaitForExitAsync();
    }
}
