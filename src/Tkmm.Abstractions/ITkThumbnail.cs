namespace Tkmm.Abstractions;

public interface ITkThumbnail
{
    public static Func<Stream, object>? CreateBitmap { get; set; }

    string ThumbnailPath { get; }

    object? Bitmap { get; set; }

    bool IsResolved { get; set; }
}