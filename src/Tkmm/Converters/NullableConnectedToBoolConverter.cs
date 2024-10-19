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
            return network.Connected;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}