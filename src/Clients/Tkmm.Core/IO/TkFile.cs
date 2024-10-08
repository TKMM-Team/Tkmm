using Revrs.Buffers;
using TotkCommon;

namespace Tkmm.Core.IO;

public static class TkFile
{
    public static ArraySegmentOwner<byte> OpenReadAndDecompress(string file, out int zsDictionaryId)
    {
        using FileStream fs = File.OpenRead(file);
        int size = Convert.ToInt32(fs.Length);
        ArraySegmentOwner<byte> buffer = ArraySegmentOwner<byte>.Allocate(size);
        _ = fs.Read(buffer.Segment);

        if (Zstd.IsCompressed(buffer.Segment)) {
            int decompressedSize = Zstd.GetDecompressedSize(buffer.Segment);
            ArraySegmentOwner<byte> decompressed = ArraySegmentOwner<byte>.Allocate(decompressedSize);
            Totk.Zstd.Decompress(buffer.Segment, decompressed.Segment, out zsDictionaryId);
            buffer.Dispose();
            return decompressed;
        }

        zsDictionaryId = -1;
        return buffer;
    }
}