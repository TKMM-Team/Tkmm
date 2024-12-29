using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using Tkmm.Core;
using TkSharp.Core.Models;

namespace Tkmm.Converters;

public class SelectedOptionsToGroupConverter : IValueConverter
{
    public static readonly SelectedOptionsToGroupConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TkModOptionGroup group || TKMM.ModManager.GetCurrentProfile().Selected is not TkProfileMod target) {
            return null;
        }
        
        if (!target.SelectedOptions.TryGetValue(group, out ObservableCollection<TkModOption>? selection)) {
            target.SelectedOptions[group] = selection = [];
        }

        return selection;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}