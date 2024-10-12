using Tkmm.Abstractions.IO.Buffers;

namespace Tkmm.Abstractions.Services;

public interface IMerger
{
    ValueTask Merge(RentedBuffers<byte> inputs, Stream output, CancellationToken ct = default);
    
    ValueTask Merge(RentedBuffers<byte> inputs, ArraySegment<byte> vanillaData, Stream output, CancellationToken ct = default);
    
    bool IsKnownFile(in TkFileInfo fileInfo);
}