using System.Globalization;
using Avalonia.Data.Converters;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Converters;

public sealed class GameBananaSortModeLocaleConverter : IValueConverter
{
    public static readonly GameBananaSortModeLocaleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GameBananaSortMode sortMode) {
            return value;
        }

        return Locale[$"GameBanana_SortMode_{sortMode}"];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
