namespace Tkmm.Core.Abstractions.Mergers;

public interface ITkMerger
{
    static abstract ValueTask Merge(ArraySegment<byte>[] inputs, Stream output, CancellationToken ct);
}