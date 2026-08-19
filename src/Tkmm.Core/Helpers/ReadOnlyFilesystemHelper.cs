namespace Tkmm.Core.Helpers;

public static class ReadOnlyFilesystemHelper
{
    public static bool IsReadOnlyFilesystemException(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException) {
            switch (current) {
                case UnauthorizedAccessException:
                case IOException { Message: var message }
                    when message.Contains("read-only", StringComparison.OrdinalIgnoreCase):
                    return true;
            }
        }

        return false;
    }
}