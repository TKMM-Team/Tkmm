namespace Tkmm.Abstractions;

public interface ITkModChangelog
{
    /// <summary>
    /// The unique ID of this changelog. 
    /// </summary>
    Ulid Id { get; }
    
    /// <summary>
    /// The tracked files contained in this changelog.
    /// </summary>
    IDictionary<string, ChangelogEntry> Manifest { get; }
}

public enum ChangelogType
{
    Merge,
    Copy,
    Ignore
}

public record struct ChangelogEntry(ChangelogType Type, TkFileAttributes Attributes, int ZsDictionaryId);