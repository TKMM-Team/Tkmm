using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Revrs.Buffers;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.IO;
using Tkmm.Core.Abstractions.Parsers;
using TotkCommon;

namespace Tkmm.IO.Desktop;

public class DesktopTkFileSystem(ITkModParserManager modParserManager) : ITkFileSystem
{
    private static readonly string _modsFolder = Path.Combine(AppContext.BaseDirectory, ".mods");
    private readonly ITkModParser _systemModParser = modParserManager.GetSystemParser();
    
    public ValueTask<T?> GetMetadata<T>(string metadataName, JsonTypeInfo<T>? typeInfo = null)
    {
        return GetJsonMetadata(
            Path.Combine(AppContext.BaseDirectory, metadataName),
            typeInfo);
    }

    public Task SetMetadata<T>(T metadata, string metadataName, JsonTypeInfo<T>? typeInfo = null)
    {
        return SaveJsonMetadata(
            metadata,
            Path.Combine(AppContext.BaseDirectory, metadataName),
            typeInfo); 
    }

    public Stream OpenModFile(ITkModChangelog mod, string fileName)
    {
        string targetFile = Path.Combine(_modsFolder, mod.Id.ToString(), fileName);
        return File.OpenRead(targetFile);
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

    private static Task SaveJsonMetadata<T>(T metadata, string targetFile, JsonTypeInfo<T>? typeInfo = null)
    {
        if (Path.GetDirectoryName(targetFile) is string folder) {
            Directory.CreateDirectory(folder);
        }

        using FileStream fs = File.Create(targetFile);
        return typeInfo switch {
            null => JsonSerializer.SerializeAsync(fs, metadata),
            _ => JsonSerializer.SerializeAsync(fs, metadata, typeInfo)
        };
    }

    public async ValueTask<TList> GetMods<TList>(Func<ITkMod, ValueTask>? initializeMod) where TList : IList<ITkMod>, new()
    {
        TList mods = [];
        if (!Directory.Exists(_modsFolder)) {
            goto Result;
        }

        foreach (string modFolder in Directory.EnumerateDirectories(_modsFolder).Where(x => File.Exists(Path.Combine(x, "info.json")))) {
            ITkMod? target = await _systemModParser.Parse(modFolder);
            if (target is null) {
                continue;
            }
            
            mods.Add(target);
            
            if (initializeMod is not null) {
                await initializeMod(target);
            }
        }
        
    Result:
        return mods;
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