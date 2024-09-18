namespace Tkmm.Core.Abstractions;

public enum ChangelogType
{
    Merge,
    Copy,
    Ignore
}

public record ChangelogEntry(ChangelogType Type);