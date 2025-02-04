using Avalonia.Data.Converters;
using Avalonia;
using System;
using System.Globalization;

namespace Tkmm.Converters;
    
public class ClassToTemplateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string classType)
        {
            return classType switch
            {
                "dropdown" => Application.Current.Resources["DropdownTemplate"],
                "scale" => Application.Current.Resources["SliderTemplate"],
                "bool" => Application.Current.Resources["CheckboxTemplate"],
                _ => null
            };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 