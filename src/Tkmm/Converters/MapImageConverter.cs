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
        if (value is string map) {
            if (!_cache.TryGetValue(map, out var bitmap)) {
                using Stream stream = AssetLoader.Open(new($"avares://Tkmm/Assets/Maps/{map}.jpg"));
                bitmap = new Bitmap(stream);
            }

            return bitmap;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
