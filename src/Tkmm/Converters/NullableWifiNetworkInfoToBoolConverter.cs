using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Tkmm.Managers;

namespace Tkmm.Converters
{
    public class NullableWifiNetworkInfoToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Connman.WifiNetworkInfo networkInfo)
            {
                return networkInfo.SavedPassword;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}