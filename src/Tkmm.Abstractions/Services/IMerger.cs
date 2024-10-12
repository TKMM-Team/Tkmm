namespace Tkmm.Abstractions.Services;

public interface IMerger
{
    ValueTask Merge(IEnumerable<ArraySegment<byte>> inputs,
        Func<ValueTask<Stream>> getOutput, CancellationToken ct = default);
    
    ValueTask Merge(IEnumerable<ArraySegment<byte>> inputs, ArraySegment<byte> vanilla,
        Func<ValueTask<Stream>> getOutput, CancellationToken ct = default);
    
    bool IsKnownFile(in TkFileInfo fileInfo);
}