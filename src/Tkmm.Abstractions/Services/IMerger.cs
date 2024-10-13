using Tkmm.Abstractions.IO.Buffers;

namespace Tkmm.Abstractions.Services;

public interface IMerger
{
    ValueTask Merge(RentedBuffers<byte> inputs, Stream output,
        ITkResourceSizeTable resourceSizeTable, CancellationToken ct = default);
    
    ValueTask Merge(RentedBuffers<byte> inputs, ArraySegment<byte> vanillaData, Stream output,
        ITkResourceSizeTable resourceSizeTable, CancellationToken ct = default);
    
    bool IsKnownFile(in TkFileInfo fileInfo);
}