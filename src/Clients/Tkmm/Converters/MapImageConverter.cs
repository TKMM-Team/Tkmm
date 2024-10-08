using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Tkmm.Converters;

public class MapImageConverter : IValueConverter
{
    private readonly Dictionary<string, Bitmap> _cache = [];
    
    public static MapImageConverter Shared { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string map) {
            return null;
        }

        if (_cache.TryGetValue(map, out Bitmap? bitmap)) {
            return bitmap;
        }

        using Stream stream = AssetLoader.Open(new Uri($"avares://Tkmm/Assets/Maps/{map}.jpg"));
        return _cache[map] = new Bitmap(stream);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
