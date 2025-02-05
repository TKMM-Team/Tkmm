namespace Tkmm.Core.Attributes;

public enum PathType
{
    File,
    Folder,
    FileOrFolder
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class PathCollectionOptionsAttribute(PathType type) : Attribute
{
    public PathType Type { get; } = type;
}