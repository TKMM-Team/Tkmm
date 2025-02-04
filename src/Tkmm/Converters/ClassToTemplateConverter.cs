using Avalonia.Data.Converters;
using Avalonia;
using System;
using System.Globalization;
using Avalonia.Controls;

namespace Tkmm.Converters;
    
public class ClassToTemplateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string classType)
        {
            var resources = parameter as IResourceDictionary ?? Application.Current.Resources;
            return classType switch
            {
                "dropdown" => resources["DropdownTemplate"],
                "scale"    => resources["SliderTemplate"],
                "bool"     => resources["CheckboxTemplate"],
                _          => null
            };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 