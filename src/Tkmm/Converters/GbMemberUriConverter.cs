using System.Globalization;
using Avalonia.Data.Converters;
using Tkmm.Helpers;

namespace Tkmm.Converters;

public sealed class GbMemberUriConverter : IValueConverter
{
    public static readonly GbMemberUriConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => GameBananaUriHelper.ToMemberUri(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
