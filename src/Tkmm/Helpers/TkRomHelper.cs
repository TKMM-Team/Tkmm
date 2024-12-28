using LibHac.Common.Keys;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using TkSharp.Extensions.LibHac;
using TkSharp.Extensions.LibHac.Extensions;

namespace Tkmm.Helpers;

public static class TkRomHelper
{
    public static IEnumerable<(string FilePath, string Version)> GetTotkRomFiles(IEnumerable<string> romFolderPaths, KeySet keys)
    {
        foreach (string target in romFolderPaths.SelectMany(dir => Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))) {
            ReadOnlySpan<char> extension = Path.GetExtension(target.AsSpan());
            if (extension is not (".nsp" or ".xci")) {
                continue;
            }

            using LocalStorage storage = new(target, FileAccess.Read);
            using SwitchFs fs = storage.GetSwitchFs(target, keys);

            if (fs.Applications.TryGetValue(PackedTkRom.EX_KING_APP_ID, out Application? app)) {
                yield return (FilePath: target, app.DisplayVersion);
            }
        }
    }
}