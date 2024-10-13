using Tkmm.Abstractions;
using Tkmm.Abstractions.Providers;
using Tkmm.Abstractions.Services;
using Tkmm.Common.ChangelogBuilders;

namespace Tkmm.Common.Providers;

public sealed class TkChangelogBuilderProvider : IChangelogBuilderProvider
{
    public IChangelogBuilder? GetChangelogBuilder(in TkFileInfo fileInfo)
    {
        return fileInfo switch {
            // TODO: GDL changelog builder
            // TODO: RSDB changelog builder
            { Extension: ".bfarc" or ".bkres" or ".blarc" or ".genvb" or ".pack" or ".ta" } => SarcChangelogBuilder.Instance,
            { Extension: ".byml" or ".bgyml" } => BymlChangelogBuilder.Instance,
            { Extension: ".msbt" } => MsbtChangelogBuilder.Instance,
            _ => null
        };
    }
}