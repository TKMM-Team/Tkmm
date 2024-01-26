namespace Tkmm.Core.Generics;

public interface IModItem
{
    public string Name { get; set; }
    public string? ThumbnailUri { get; set; }
    public string SourceFolder { get; }
}
