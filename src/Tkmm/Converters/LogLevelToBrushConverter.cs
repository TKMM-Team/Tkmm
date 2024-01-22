using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;
using Tkmm.Core.Models;

namespace Tkmm.Converters;

public class LogLevelToBrushConverter : IValueConverter
{
    private static readonly BrushConverter _brushConverter = new();
    private static readonly object? _default = Brushes.Transparent;
    private static readonly object? _info = _brushConverter.ConvertFromInvariantString("#2E5FC9");
    private static readonly object? _debug = _brushConverter.ConvertFromInvariantString("#A86032");
    private static readonly object? _warning = _brushConverter.ConvertFromInvariantString("#C9962E");
    private static readonly object? _error = _brushConverter.ConvertFromInvariantString("#C9402E");

    private static readonly Lazy<LogLevelToBrushConverter> _shared = new(() => new());
    public static LogLevelToBrushConverter Shared => _shared.Value;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogLevel logLevel) {
            return logLevel switch {
                LogLevel.Debug => _debug,
                LogLevel.Info => _info,
                LogLevel.Warning => _warning,
                LogLevel.Error => _error,
                _ => _default
            };
        }

        return _default;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == _info) {
            return LogLevel.Info;
        }
        else if (value == _debug) {
            return LogLevel.Debug;
        }
        else if (value == _warning) {
            return LogLevel.Warning;
        }
        else if (value == _error) {
            return LogLevel.Error;
        }

        return LogLevel.Default;
    }
}
