using Revrs.Buffers;
using Tkmm.Core.Abstractions;

namespace Tkmm.Core;

internal class TkModChangelog : TkItem, ITkModChangelog
{
    public IDictionary<string, ChangelogEntry> Manifest { get; } = new Dictionary<string, ChangelogEntry>();

    public ArraySegmentOwner<byte> GetChangelogData(string fileName)
    {
        return TKMM.FS.OpenReadAndDecompress(fileName, out _);
    }
}