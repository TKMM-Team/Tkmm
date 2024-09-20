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
}