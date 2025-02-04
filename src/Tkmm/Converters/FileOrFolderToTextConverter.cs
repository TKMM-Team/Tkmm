using System.Globalization;
using Avalonia.Data.Converters;
using Tkmm.Core.Models;

namespace Tkmm.Converters;

public sealed class FileOrFolderToTextConverter : IValueConverter
{
    public static readonly FileOrFolderToTextConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch {
            string path => (FileOrFolder)path,
            FileOrFolder fileOrFolder => fileOrFolder.Path,
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch {
            string path => (FileOrFolder)path,
            FileOrFolder fileOrFolder => fileOrFolder.Path,
            _ => null
        };
    }
}