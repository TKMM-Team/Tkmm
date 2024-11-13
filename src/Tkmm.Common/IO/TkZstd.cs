using System.Buffers;
using System.Diagnostics;
using CommunityToolkit.HighPerformance.Buffers;
using Revrs;
using Revrs.Extensions;
using SarcLibrary;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.IO.Buffers;
using ZstdSharp;

namespace Tkmm.Common.IO;

public sealed class TkZstd : IZstd
{
    private const uint DICT_MAGIC = 0xEC30A437;
    private const uint SARC_MAGIC = 0x43524153;

    private readonly Decompressor _defaultDecompressor = new();
    private readonly Dictionary<int, Decompressor> _decompressors = [];
    private readonly Compressor _defaultCompressor;
    private readonly Dictionary<int, Compressor> _compressors = [];

    public TkZstd(Stream zsDicPack, long streamLength)
    {
        _defaultCompressor = new Compressor(CompressionLevel);
        LoadDictionaries(zsDicPack, streamLength);
    }

    private int _compressionLevel = 7;

    public int CompressionLevel {
        get => _compressionLevel;
        set {
            _compressionLevel = value;
            _defaultCompressor.Level = _compressionLevel;

            foreach ((int _, Compressor compressor) in _compressors) {
                compressor.Level = value;
            }
        }
    }

    public void Decompress(ReadOnlySpan<byte> data, Span<byte> dst, out int zsDictionaryId)
    {
        if (!IZstd.IsCompressed(data)) {
            zsDictionaryId = -1;
            return;
        }

        zsDictionaryId = IZstd.GetDictionaryId(data);
        lock (_decompressors) {
            if (_decompressors.TryGetValue(zsDictionaryId, out Decompressor? decompressor)) {
                decompressor.Unwrap(data, dst);
                return;
            }
        }

        lock (_defaultDecompressor) {
            _defaultDecompressor.Unwrap(data, dst);
        }
    }

    public RentedBuffer<byte> Compress(ReadOnlySpan<byte> data, int zsDictionaryId = -1)
    {
        int bounds = Compressor.GetCompressBound(data.Length);
        RentedBuffer<byte> result = RentedBuffer<byte>.Allocate(bounds);
        int size = Compress(data, result.Span, zsDictionaryId);
        result.Resize(size);
        return result;
    }

    public int Compress(ReadOnlySpan<byte> data, Span<byte> dst, int zsDictionaryId = -1)
    {
        return _compressors.TryGetValue(zsDictionaryId, out Compressor? compressor) switch {
            true => compressor.Wrap(data, dst),
            false => _defaultCompressor.Wrap(data, dst)
        };
    }

    public async ValueTask LoadDictionariesAsync(Stream stream, long streamLength)
    {
        int size = Convert.ToInt32(streamLength);
        using RentedBuffer<byte> buffer = RentedBuffer<byte>.Allocate(size);
        int read = await stream.ReadAsync(buffer.Memory);
        Debug.Assert(size == read);
        LoadDictionaries(buffer.Span);
    }

    public void LoadDictionaries(Stream stream, long streamLength)
    {
        int size = Convert.ToInt32(streamLength);
        using SpanOwner<byte> buffer = SpanOwner<byte>.Allocate(size);
        int read = stream.Read(buffer.Span);
        Debug.Assert(size == read);
        LoadDictionaries(buffer.Span);
    }

    public void LoadDictionaries(Span<byte> data)
    {
        byte[]? decompressedBuffer = null;

        if (IZstd.IsCompressed(data)) {
            int decompressedSize = IZstd.GetDecompressedSize(data);
            decompressedBuffer = ArrayPool<byte>.Shared.Rent(decompressedSize);
            Span<byte> decompressed = decompressedBuffer.AsSpan()[..decompressedSize];
            Decompress(data, decompressed, out _);
            data = decompressed;
        }

        if (TryLoadDictionary(data)) {
            return;
        }

        if (data.Length < 8 || data.Read<uint>() != SARC_MAGIC) {
            return;
        }

        RevrsReader reader = new(data);
        ImmutableSarc sarc = new(ref reader);
        foreach ((string _, Span<byte> sarcFileData) in sarc) {
            TryLoadDictionary(sarcFileData);
        }

        if (decompressedBuffer is not null) {
            ArrayPool<byte>.Shared.Return(decompressedBuffer);
        }
    }

    private bool TryLoadDictionary(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 8 || buffer.Read<uint>() != DICT_MAGIC) {
            return false;
        }

        Decompressor decompressor = new();
        decompressor.LoadDictionary(buffer);
        _decompressors[buffer[4..8].Read<int>()] = decompressor;

        Compressor compressor = new(CompressionLevel);
        compressor.LoadDictionary(buffer);
        _compressors[buffer[4..8].Read<int>()] = compressor;

        return true;
    }
}