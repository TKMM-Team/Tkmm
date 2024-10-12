namespace Tkmm.Abstractions.Services;

public interface IMerger
{
    ValueTask Merge(IEnumerable<ArraySegment<byte>> inputs, Func<Stream> getOutput, CancellationToken ct = default);
    
    bool IsKnownFile(in TkFileInfo fileInfo);
}