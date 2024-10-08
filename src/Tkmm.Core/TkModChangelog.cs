using System.Diagnostics;
using Revrs.Buffers;
using Tkmm.Core.Abstractions;

namespace Tkmm.Core;

internal class TkModChangelog : TkItem, ITkModChangelog
{
    public IDictionary<string, ChangelogEntry> Manifest { get; } = new Dictionary<string, ChangelogEntry>();

    public ArraySegmentOwner<byte> GetChangelogData(string fileName)
    {
        using Stream stream = TKMM.FS.OpenModFile(this, fileName);
        int size = Convert.ToInt32(stream.Length);
        
        ArraySegmentOwner<byte> result = ArraySegmentOwner<byte>.Allocate(size);
        int read = stream.Read(result.Segment);
        
        Debug.Assert(read == size);
        
        return result;
    }
}