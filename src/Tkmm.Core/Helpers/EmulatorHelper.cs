using System.Runtime.CompilerServices;
using TotkCommon;

namespace Tkmm.Core.Helpers;

internal static class EmulatorHelper
{
    private const string MOD_FOLDER_NAME = "TKMM";
    
    private static readonly string _yuzuDefaultModsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu", "load", Totk.TITLE_ID, MOD_FOLDER_NAME);
    private static readonly string _yuzuConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu", "config", "qt-config.ini");
    
    public static string GetYuzuModsPath()
    {
        if (!File.Exists(_yuzuConfigPath)) {
            return _yuzuDefaultModsPath;
        }

        using FileStream fs = File.OpenRead(_yuzuConfigPath);
        using StreamReader reader = new(fs);

        const string prefix = "load_directory=";

        while (reader.ReadLine() is string line) {
            if (line.StartsWith(prefix)) {
                return Path.Combine(line[prefix.Length..], Totk.TITLE_ID, MOD_FOLDER_NAME);
            }
        }

        return _yuzuDefaultModsPath;
    }

    private static readonly string _ryujinxDefaultModsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx", "mods", "contents", Totk.TITLE_ID.ToLower(), MOD_FOLDER_NAME);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetRyujinxModsPath()
    {
        return _ryujinxDefaultModsPath;
    }
}