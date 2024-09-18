namespace Tkmm.Core.Abstractions.ChangelogBuilders;

public interface ITkChangelogBuilder<TChangelog>
{
    static abstract ValueTask<ChangelogBuildResult<TChangelog>> LogChanges(ArraySegment<byte> input,
        in TkFileInfo info, CancellationToken ct);

    static abstract ValueTask WriteChangelog(TChangelog changelog, in TkFileInfo info);
}