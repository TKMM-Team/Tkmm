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

            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;
            
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return d.ToString("0.##", CultureInfo.InvariantCulture);
            
            return value?.ToString() ?? "0";
        }
    }
} 