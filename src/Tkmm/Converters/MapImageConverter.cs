using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Globalization;

namespace Tkmm.Converters;

public class MapImageConverter : IValueConverter
{
    private static readonly Lazy<MapImageConverter> _shared = new(() => new());
    public static MapImageConverter Shared => _shared.Value;

    private readonly Dictionary<string, Bitmap> _cache = [];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string map) {
            return null;
        }

        if (_cache.TryGetValue(map, out Bitmap? bitmap)) {
            return bitmap;
        }

        using Stream stream = AssetLoader.Open(new($"avares://Tkmm/Assets/Maps/{map}.jpg"));
        return _cache[map] = bitmap = new(stream);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
