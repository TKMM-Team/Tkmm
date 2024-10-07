namespace Tkmm.Core.Abstractions.Common;

public interface IThumbnail
{
    public static Func<Stream, object>? CreateBitmap { get; set; }
    
    string ThumbnailPath { get; }
    bool IsResolved { get; set; }
    object? Thumbnail { get; set; }
}