using Avalonia.Data.Converters;
using Avalonia;
using System;
using System.Globalization;

public class ClassToTemplateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string classType)
        {
            return classType switch
            {
                "dropdown" => Avalonia.Application.Current.Resources["DropdownTemplate"],
                "scale" => Avalonia.Application.Current.Resources["SliderTemplate"],
                "bool" => Avalonia.Application.Current.Resources["CheckboxTemplate"],
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