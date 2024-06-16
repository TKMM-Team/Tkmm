using Tkmm.Core.Models.GameBanana;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModReaders;

public class GameBananaModReader : IModReader
{
    private const string GB_MODS_URL = "https://gamebanana.com/mods/";

    public bool IsValid(string path)
    {
        return path.StartsWith(GB_MODS_URL) || ulong.TryParse(path, out _);
    }

    public async Task<Mod> Read(Stream? input, string path, Guid? modId)
    {
        string id;

        if (path.StartsWith(GB_MODS_URL)) {
            id = Path.GetRelativePath(GB_MODS_URL, path)
                .Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
        }
        else if (int.TryParse(path, out _)) {
            id = path;
        }
        else {
            throw new ArgumentException($"""
                Invalid url or mod id: '{path}'
                """, nameof(path));
        }

        return await GameBananaMod.FromId(id);
    }
}
