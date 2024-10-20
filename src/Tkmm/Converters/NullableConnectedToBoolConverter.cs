using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tkmm.Converters;

public class NullableConnectedToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Managers.Connman.WifiNetworkInfo network)
        {
            bool isConnected = network.Connected;
            if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                return !isConnected;
            }
            return isConnected;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}