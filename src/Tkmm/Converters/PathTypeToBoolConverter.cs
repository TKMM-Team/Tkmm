using System.Globalization;
using Avalonia.Data.Converters;
using Tkmm.Core.Attributes;

namespace Tkmm.Converters;

public sealed class PathTypeToBoolConverter : IValueConverter
{
    public static readonly PathTypeToBoolConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PathType type) {
            return parameter switch {
                "file" => type is PathType.File or PathType.FileOrFolder,
                "folder" => type is PathType.Folder or PathType.FileOrFolder,
                _ => true
            };
        }

        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}