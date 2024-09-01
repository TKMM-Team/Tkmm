using Avalonia.Data.Converters;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Tkmm.Converters;

public class SemicolonDelimitedListConverter : IValueConverter
{
    private static readonly Lazy<SemicolonDelimitedListConverter> _shared = new(() => new());
    public static SemicolonDelimitedListConverter Shared => _shared.Value;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> collection) {
            return string.Join(';', collection);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text) {
            return new ObservableCollection<string>(text.Split(';'));
        }

        return new ObservableCollection<string>();
    }
}
