using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Revrs.Buffers;

namespace Tkmm.Core.Abstractions.IO;

public interface IRomfs
{
    /// <summary>
    /// Reads and decompresses the requested vanilla file.
    /// </summary>
    /// <param name="fileName">The relative path to the vanilla file.</param>
    /// <returns></returns>
    ArraySegmentOwner<byte> GetVanilla(string fileName) => GetVanilla(fileName, out _);
    
    /// <summary>
    /// Reads and decompresses the requested vanilla file.
    /// </summary>
    /// <param name="fileName">The relative path to the vanilla file.</param>
    /// <param name="zsDictionaryId">The ID of the dictionary used to compress the requested vanilla file.</param>
    /// <returns></returns>
    ArraySegmentOwner<byte> GetVanilla(string fileName, out int zsDictionaryId);
    
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