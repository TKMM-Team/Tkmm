using Tkmm.Abstractions.IO.Buffers;
using TotkCommon;

namespace Tkmm.Core.IO;

public static class TkFile
{
    public static RentedBuffer<byte> OpenReadAndDecompress(string file, out int zsDictionaryId)
    {
        if (!File.Exists(file)) {
            zsDictionaryId = -1;
            return default;
        }
        
        using FileStream fs = File.OpenRead(file);
        int size = Convert.ToInt32(fs.Length);
        RentedBuffer<byte> buffer = RentedBuffer<byte>.Allocate(size);
        _ = fs.Read(buffer.Span);

        if (Zstd.IsCompressed(buffer.Segment)) {
            int decompressedSize = Zstd.GetDecompressedSize(buffer.Segment);
            RentedBuffer<byte> decompressed = RentedBuffer<byte>.Allocate(decompressedSize);
            Totk.Zstd.Decompress(buffer.Span, decompressed.Span, out zsDictionaryId);
            buffer.Dispose();
            return decompressed;
        }

        zsDictionaryId = -1;
        return buffer;
    }
}