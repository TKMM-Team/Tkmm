using Tkmm.Abstractions.Providers;
using Tkmm.Abstractions.Services;
using Tkmm.Common.Mergers;

namespace Tkmm.Common.Providers;

public sealed class TkMergerProvider : IMergerProvider
{
    public IMerger? GetMerger(in ReadOnlySpan<char> canonical)
    {
        return canonical switch {
            // TODO: GDL Merger
            // TODO: RSDB Merger
            _ => Path.GetExtension(canonical) switch {
                ".bfarc" or ".bkres" or ".blarc" or ".genvb" or ".pack" or ".ta" => SarcMerger.Instance,
                ".byml" or ".bgyml" => BymlMerger.Instance,
                ".msbt" => MsbtMerger.Instance,
                _ => null
            }
        };
    }
}