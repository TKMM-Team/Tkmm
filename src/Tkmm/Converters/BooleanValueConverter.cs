using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Tkmm.Converters
{
    public class BooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return false;

            if (bool.TryParse(value.ToString(), out bool result))
                return result;
            
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b.ToString().ToLower();
            
            return "false";
        }
    }
} 