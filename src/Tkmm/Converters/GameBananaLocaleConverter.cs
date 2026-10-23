using System.Globalization;
using Avalonia.Data.Converters;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Converters;

public sealed class GameBananaLocaleConverter : IValueConverter
{
    public static readonly GameBananaLocaleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch {
            GameBananaSortMode sortMode => Locale[$"GameBanana_SortMode_{sortMode}"],
            GameBananaSubmissionType submissionType => Locale[$"GameBanana_SubmissionType_{submissionType}"],
            _ => value
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}