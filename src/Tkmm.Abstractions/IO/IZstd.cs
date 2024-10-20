using Revrs.Extensions;
using Tkmm.Abstractions.IO.Buffers;

namespace Tkmm.Abstractions.IO;

public interface IZstd
{
    public const uint ZSTD_MAGIC = 0xFD2FB528;

    int CompressionLevel { get; set; }

    public byte[] Decompress(ReadOnlySpan<byte> data)
        => Decompress(data, out _);
    public byte[] Decompress(ReadOnlySpan<byte> data, out int zsDictionaryId)
    {
        int size = GetDecompressedSize(data);
        byte[] result = new byte[size];
        Decompress(data, result, out zsDictionaryId);
        return result;
    }

    public void Decompress(ReadOnlySpan<byte> data, Span<byte> dst)
        => Decompress(data, dst, out _);
    public void Decompress(ReadOnlySpan<byte> data, Span<byte> dst, out int zsDictionaryId);

    public RentedBuffer<byte> Compress(ReadOnlySpan<byte> data, int zsDictionaryId = -1);
    public int Compress(ReadOnlySpan<byte> data, Span<byte> dst, int zsDictionaryId = -1);

    public static bool IsCompressed(ReadOnlySpan<byte> data)
    {
        return data.Length > 3 &&
               data.Read<uint>() == ZSTD_MAGIC;
    }

    public static int GetDecompressedSize(Stream stream)
    {
        Span<byte> header = stackalloc byte[14];
        _ = stream.Read(header);
        return GetFrameContentSize(header);
    }

    public static int GetDecompressedSize(ReadOnlySpan<byte> data)
    {
        return GetFrameContentSize(data);
    }

    public static int GetDictionaryId(ReadOnlySpan<byte> buffer)
    {
        byte descriptor = buffer[4];
        int windowDescriptorSize = ((descriptor & 0b00100000) >> 5) ^ 0b1;
        int dictionaryIdFlag = descriptor & 0b00000011;

        return dictionaryIdFlag switch {
            0x0 => -1,
            0x1 => buffer[5 + windowDescriptorSize],
            0x2 => buffer[(5 + windowDescriptorSize)..].Read<ushort>(),
            0x3 => buffer[(5 + windowDescriptorSize)..].Read<int>(),
            _ => throw new OverflowException(
                "Two bits cannot exceed 0x3, something terrible has happened!")
        };
    }

    private static int GetFrameContentSize(ReadOnlySpan<byte> buffer)
    {
        byte descriptor = buffer[4];
        int windowDescriptorSize = ((descriptor & 0b00100000) >> 5) ^ 0b1;
        int dictionaryIdFlag = descriptor & 0b00000011;
        int frameContentFlag = descriptor >> 6;

        int offset = dictionaryIdFlag switch {
            0x0 => 5 + windowDescriptorSize,
            0x1 => 5 + windowDescriptorSize + 1,
            0x2 => 5 + windowDescriptorSize + 2,
            0x3 => 5 + windowDescriptorSize + 4,
            _ => throw new OverflowException(
                "Two bits cannot exceed 0x3, something terrible has happened!")
        };

        return frameContentFlag switch {
            0x0 => buffer[offset],
            0x1 => buffer[offset..].Read<ushort>() + 0x100,
            0x2 => buffer[offset..].Read<int>(),
            _ => throw new NotSupportedException(
                "64-bit file sizes are not supported.")
        };
    }
}