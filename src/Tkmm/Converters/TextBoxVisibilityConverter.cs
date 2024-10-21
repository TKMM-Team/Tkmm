using System.Globalization;
using Avalonia.Data.Converters;

namespace Tkmm.Converters;

public class TextBoxVisibilityConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2 && values[0] is bool savedPassword && values[1] is bool connected)
        {
            return !savedPassword && !connected;
        }
        return false;
    }
}