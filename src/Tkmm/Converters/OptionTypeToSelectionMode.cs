using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using TkSharp.Core.Models;

namespace Tkmm.Converters;

public class OptionTypeToSelectionMode : IValueConverter
{
    public static OptionTypeToSelectionMode Shared { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch {
            OptionGroupType.Multi => SelectionMode.Multiple | SelectionMode.Toggle,
            OptionGroupType.MultiRequired => SelectionMode.Multiple | SelectionMode.Toggle,
            OptionGroupType.Single => SelectionMode.Single | SelectionMode.Toggle,
            OptionGroupType.SingleRequired => SelectionMode.Single | SelectionMode.Toggle,
            _ => null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
