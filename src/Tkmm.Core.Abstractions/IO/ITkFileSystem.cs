using System.Text.Json.Serialization.Metadata;
using Revrs.Buffers;

namespace Tkmm.Core.Abstractions.IO;

public interface ITkFileSystem
{
    /// <summary>
    /// Get and deserialize system metadata.
    /// </summary>
    /// <param name="metadataName"></param>
    /// <param name="typeInfo"></param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns></returns>
    ValueTask<T?> GetMetadata<T>(string metadataName, JsonTypeInfo<T>? typeInfo = null);
    
    /// <summary>
    /// Open a <see cref="Stream"/> to the requested <paramref name="fileName"/> in the provided <paramref name="mod"/>. 
    /// </summary>
    /// <param name="mod"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Stream OpenModFile(ITkMod mod, string fileName);
    
    /// <summary>
    /// Retrieve a list of persisted mods. 
    /// </summary>
    /// <returns></returns>
    ValueTask<TList> GetMods<TList>(Func<ITkMod, ValueTask>? initializeMod = null) where TList : IList<ITkMod>, new();

    /// <summary>
    /// Open a <see cref="Stream"/> to the requested <paramref name="file"/>.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    Stream OpenRead(string file);

    /// <summary>
    /// Read and decompress the requested <paramref name="file"/>.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="zsDictionaryId">The zstd dictionary id used to decompress the <paramref name="file"/>.</param>
    /// <returns>A rented array segment containing the decompressed file contents.</returns>
    ArraySegmentOwner<byte> OpenReadAndDecompress(string file, out int zsDictionaryId);
}