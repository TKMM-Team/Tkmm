using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Tkmm.Converters
{
    public class DoubleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return 0.0;

            if (double.TryParse(value.ToString(), out double result))
                return result;
            
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "0";
        }
    }
} 