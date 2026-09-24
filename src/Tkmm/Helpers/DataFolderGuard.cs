using Tkmm.Core;
using Tkmm.Core.Helpers;

namespace Tkmm.Helpers;

public static class DataFolderGuard
{
    public static bool IsDataFolderWritable()
    {
        var dataFolder = Path.Combine(TKMM.BaseDirectory, ".data2");

        try {
            Directory.CreateDirectory(dataFolder);
            var testFile = Path.Combine(dataFolder, ".write-test");
            File.WriteAllText(testFile, "1");
            File.Delete(testFile);
            return true;
        }
        catch (Exception ex) when (ReadOnlyFilesystemHelper.IsReadOnlyFilesystemException(ex)) {
            return false;
        }
    }
}
