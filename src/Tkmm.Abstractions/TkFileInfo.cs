namespace Tkmm.Abstractions;

[Flags]
public enum RomfsFileAttributes
{
    None = 0,
    HasZsExtension = 1,
    HasMcExtension = 2,
    IsProductFile = 4,
}

public readonly ref struct TkFileInfo(
    string filePath,
    string romfsPath,
    ReadOnlySpan<char> canonical,
    ReadOnlySpan<char> extension,
    RomfsFileAttributes attributes)
{
    public string FilePath { get; } = filePath;

    public string RomfsPath { get; } = romfsPath;

    public ReadOnlySpan<char> Canonical { get; } = canonical;

    public ReadOnlySpan<char> Extension { get; } = extension;

    public RomfsFileAttributes Attributes { get; } = attributes;
}