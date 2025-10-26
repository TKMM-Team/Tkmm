using System.Globalization;
using Avalonia.Data.Converters;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Converters;

public class GameBananaFileSorter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<GameBananaFile> files)
        {
            return files.OrderByDescending(f => f.IsTkcl).ToList();
        }
        
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
