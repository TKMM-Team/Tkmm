using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tkmm.Abstractions.IO.Buffers;

namespace Tkmm.Abstractions.IO;

public interface IRomfs
{
    IDictionary<string, string> AddressTable { get; }

    string Version { get; }
    
    IZstd Zstd { get; }

    /// <inheritdoc cref="GetVanilla(System.String,Tkmm.Abstractions.TkFileAttributes,out int)"/>
    RentedBuffer<byte> GetVanilla(ReadOnlySpan<char> canonical, TkFileAttributes attributes)
        => GetVanilla(canonical, attributes, out _);

    /// <inheritdoc cref="GetVanilla(System.String,Tkmm.Abstractions.TkFileAttributes,out int)"/>
    RentedBuffer<byte> GetVanilla(ReadOnlySpan<char> canonical, TkFileAttributes attributes, out int zsDictionaryId)
    {
        string canonicalManaged = canonical.ToString();
        return GetVanilla(canonicalManaged, attributes, out zsDictionaryId);
    }

    /// <inheritdoc cref="GetVanilla(System.String,Tkmm.Abstractions.TkFileAttributes,out int)"/>
    RentedBuffer<byte> GetVanilla(string canonical, TkFileAttributes attributes)
        => GetVanilla(canonical, attributes, out _);

    /// <summary>
    /// Reads and decompresses the requested vanilla file.
    /// </summary>
    /// <param name="canonical">The canonical file name.</param>
    /// <param name="attributes">The attributes of the file.</param>
    /// <param name="zsDictionaryId">The dictionary id used to decompress the file.</param>
    /// <returns></returns>
    RentedBuffer<byte> GetVanilla(string canonical, TkFileAttributes attributes, out int zsDictionaryId)
    {
        string fileName = AddressTable.TryGetValue(canonical, out string? versionedFileName)
            ? versionedFileName
            : canonical;

        if (attributes.HasFlag(TkFileAttributes.HasZsExtension)) {
            fileName += ".zs";
        }

        return GetVanilla(fileName, out zsDictionaryId);
    }

    /// <inheritdoc cref="GetVanilla(System.ReadOnlySpan{char},Tkmm.Abstractions.TkFileAttributes)"/>
    RentedBuffer<byte> GetVanilla(string fileName)
        => GetVanilla(fileName, out _);

    /// <summary>
    /// Reads and decompresses the requested vanilla file.
    /// </summary>
    /// <param name="fileName">The relative path to the vanilla file.</param>
    /// <param name="zsDictionaryId">The ID of the dictionary used to compress the requested vanilla file.</param>
    /// <returns></returns>
    RentedBuffer<byte> GetVanilla(string fileName, out int zsDictionaryId);

    /// <summary>
    /// Decomrpesses the input stream using the loaded zstd dictionaries.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="zsDictionaryId"></param>
    /// <returns></returns>
    RentedBuffer<byte> Decompress(in Stream stream, out int zsDictionaryId);

    /// <summary>
    /// Log information about the <see cref="IRomfs"/> implementation. 
    /// </summary>
    /// <returns></returns>
    void LogInfo(ILogger logger);

    /// <summary>
    /// Check if the <see cref="IRomfs"/> is in a valid state and correctly configured. 
    /// </summary>
    /// <returns></returns>
    public bool IsStateValid([MaybeNullWhen(true)] out string invalidReason);
}