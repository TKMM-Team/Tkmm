using Revrs.Buffers;

namespace Tkmm.Core.Abstractions;

public interface ITkModChangelog
{
    IDictionary<string, ChangelogEntry> Manifest { get; }

    ArraySegmentOwner<byte> GetChangelogData(string fileName);
}