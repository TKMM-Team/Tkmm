namespace Tkmm.Core.Generics;

public interface IModItem
{
    public Guid Id { get; }
    public string Name { get; set; }
    public string? ThumbnailUri { get; set; }
    public string SourceFolder { get; }
}
