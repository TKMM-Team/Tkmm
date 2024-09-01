using Avalonia.Controls;
using Avalonia.Data.Converters;
using System.Globalization;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Converters;

public class OptionTypeToSelectionMode : IValueConverter
{
    private static readonly Lazy<OptionTypeToSelectionMode> _shared = new(() => new());
    public static OptionTypeToSelectionMode Shared => _shared.Value;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ModOptionGroupType type) {
            return type switch {
                ModOptionGroupType.Multi => SelectionMode.Multiple | SelectionMode.Toggle,
                ModOptionGroupType.MultiRequired => SelectionMode.Multiple | SelectionMode.Toggle | SelectionMode.AlwaysSelected,
                ModOptionGroupType.Single => SelectionMode.Single | SelectionMode.Toggle,
                ModOptionGroupType.SingleRequired => SelectionMode.Single | SelectionMode.Toggle | SelectionMode.AlwaysSelected,
                _ => null
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
