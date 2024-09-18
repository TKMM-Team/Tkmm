namespace Tkmm.Core.Abstractions.Common;

public interface IThumbnail
{
    string ThumbnailPath { get; }
    object? Thumbnail { get; }
}