using Revrs.Buffers;
using Tkmm.Core.Abstractions.IO;
using TotkCommon;

namespace Tkmm.IO.Desktop;

public sealed class ExtractedRomfs(ITkFileSystem fs) : IRomfs
{
    public ArraySegmentOwner<byte> GetVanilla(string fileName, out int zsDictionaryId)
    {
        string absoluteFilePath = Path.Combine(Totk.Config.GamePath, fileName);
        return fs.OpenReadAndDecompress(absoluteFilePath, out zsDictionaryId);
    }
}