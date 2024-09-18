using System.Text.Json.Serialization.Metadata;

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
    ValueTask<T> GetMetadata<T>(string metadataName, JsonTypeInfo<T>? typeInfo = null);
}