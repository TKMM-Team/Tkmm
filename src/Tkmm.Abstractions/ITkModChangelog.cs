namespace Tkmm.Abstractions;

public interface ITkModChangelog
{
    /// <summary>
    /// The unique ID of this changelog. 
    /// </summary>
    Ulid Id { get; }
    
    /// <summary>
    /// The tracked romfs files contained in this mod changelog.
    /// </summary>
    IDictionary<string, ChangelogEntry> Manifest { get; }
    
    /// <summary>
    /// Exefs patches contained in this mod changelog.
    /// </summary>
    IList<TkPatch> Patches { get; }
    
    /// <summary>
    /// The tracked subsdk files contained in this mod changelog.
    /// </summary>
    IList<string> SubSdkFiles { get; }
    
    /// <summary>
    /// The tracked cheats contained in this mod changelog.
    /// </summary>
    IList<string> Cheats { get; }
}

public enum ChangelogType
{
    Merge,
    Copy,
    Ignore
}

public record struct ChangelogEntry(ChangelogType Type, TkFileAttributes Attributes, int ZsDictionaryId);