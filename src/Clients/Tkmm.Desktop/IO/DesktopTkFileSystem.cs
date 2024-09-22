using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Revrs.Buffers;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.IO;
using Tkmm.Core.Abstractions.Parsers;
using TotkCommon;

namespace Tkmm.Desktop.IO;

public class DesktopTkFileSystem(ITkModParserManager modParserManager) : ITkFileSystem
{
    private readonly ITkModParser _systemModParser = modParserManager.GetSystemParser();
    
    public ValueTask<T?> GetMetadata<T>(string metadataName, JsonTypeInfo<T>? typeInfo = null)
    {
        if (File.Exists(metadataName)) {
            return GetJsonMetadata(metadataName, typeInfo);
        }
        
        return metadataName switch {
            "mods" => GetMods<T>(),
            _ => GetJsonMetadata(Path.Combine(AppContext.BaseDirectory, metadataName), typeInfo)
        };
    }

    private static ValueTask<T?> GetJsonMetadata<T>(string targetFile, JsonTypeInfo<T>? typeInfo = null)
    {
        if (!File.Exists(targetFile)) {
            return default;
        }

        using FileStream fs = File.OpenRead(targetFile);
        return typeInfo switch {
            null => JsonSerializer.DeserializeAsync<T>(fs),
            _ => JsonSerializer.DeserializeAsync(fs, typeInfo)
        };
    }

    private async ValueTask<T?> GetMods<T>()
    {
        string targetFolder = Path.Combine(AppContext.BaseDirectory, "mods");
        
        IList<ITkMod> mods = [];
        if (!Directory.Exists(targetFolder)) {
            goto Result;
        }

        foreach (string modFolder in Directory.EnumerateDirectories(targetFolder).Where(x => File.Exists(Path.Combine(x, "info.json")))) {
            ITkMod target = await _systemModParser.Parse(modFolder);
            mods.Add(target);
        }
        
    Result:
        return (T)mods;
    }

    public ArraySegmentOwner<byte> OpenReadAndDecompress(string file, out int zsDictionaryId)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stream OpenRead(string file)
    {
        return File.OpenRead(file);
    }
}