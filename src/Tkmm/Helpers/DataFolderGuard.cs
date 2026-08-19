namespace Tkmm.Helpers;

using Tkmm.Core.Helpers;

public static class DataFolderGuard
{
    public static string GetApplicationBaseDirectory()
    {
#if SWITCH
        return "/storage/.tkmm";
#else
        return AppContext.BaseDirectory;
#endif
    }

    private static string GetDataFolderPath(string baseDirectory)
        => Path.Combine(baseDirectory, ".data2");

    public static bool IsDataFolderWritable(string baseDirectory)
    {
        var dataFolder = GetDataFolderPath(baseDirectory);

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