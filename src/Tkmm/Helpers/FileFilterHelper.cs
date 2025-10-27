using Avalonia.Platform.Storage;

namespace Tkmm.Helpers;

public static class FileFilterHelper
{
    public static FilePickerFileType[] ParseFilterString(string? filter = null)
    {
        if (filter != null) {
            try {
                var groups = filter.Split('|');
                var types = new FilePickerFileType[groups.Length];

                for (var i = 0; i < groups.Length; i++) {
                    var pair = groups[i].Split(':');
                    types[i] = new FilePickerFileType(pair[0]) {
                        Patterns = pair[1].Split(';')
                    };
                }

                return types;
            }
            catch {
                throw new FormatException(
                    $"Could not parse filter arguments '{filter}'.\n" +
                    $"Example: \"Yaml Files:*.yml;*.yaml|All Files:*.*\"."
                );
            }
        }

        return [];
    }
}