using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModParsers;

public class TkclModReader : IModReader
{
    public bool IsValid(string file)
    {
        return Path.GetExtension(file) == ".tkcl";
    }

    public Mod Parse(Stream input, string file)
    {
        using ZipArchive archive = new(input, mode: ZipArchiveMode.Read, leaveOpen: true);

        if (archive.Entries.FirstOrDefault(x => x.Name == "info.json")?.Open() is Stream stream) {
            if (JsonSerializer.Deserialize<Mod>(stream) is Mod mod) {
                string outputFolder = Path.Combine(Config.Shared.StorageFolder, "mods", mod.Id.ToString());
                archive.ExtractToDirectory(outputFolder);
                return mod;
            }
        }

        throw new InvalidOperationException("""
            Invalid TKCL file, metadata could not be found.
            """);
    }
}
