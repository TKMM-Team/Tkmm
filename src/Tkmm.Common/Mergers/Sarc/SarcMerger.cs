using Tkmm.Abstractions;
using Tkmm.Abstractions.IO.Buffers;
using Tkmm.Abstractions.Services;

// ReSharper disable once CheckNamespace
namespace Tkmm.Common.Mergers;

public sealed class SarcMerger : IMerger
{
    public static readonly SarcMerger Instance = new();
    
    public ValueTask Merge(RentedBuffers<byte> inputs, Stream output,
        ITkResourceSizeTable resourceSizeTable, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask Merge(RentedBuffers<byte> inputs, ArraySegment<byte> vanillaData, Stream output,
        ITkResourceSizeTable resourceSizeTable, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public bool IsKnownFile(in TkFileInfo fileInfo)
    {
        return fileInfo.Extension is ".bfarc" or ".bkres" or ".blarc" or ".genvb" or ".pack" or ".ta";
    }
}