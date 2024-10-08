using Revrs.Buffers;

namespace Tkmm.Core.Abstractions;

public interface ITkModChangelog
{
    Ulid Id { get; }
    
    IDictionary<string, ChangelogEntry> Manifest { get; }

    ArraySegmentOwner<byte> GetChangelogData(string fileName);
}