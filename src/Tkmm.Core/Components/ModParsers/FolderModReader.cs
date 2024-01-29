using System.Text.Json;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModParsers;

public class FolderModReader : IModReader
{
    public bool IsValid(string path)
    {
        return Directory.Exists(path) && (
            Directory.Exists(Path.Combine(path, ModManager.ROMFS)) || Directory.Exists(Path.Combine(path, ModManager.EXEFS))
        );
    }

    public async Task<Mod> Read(Stream? _, string path)
    {
        Mod? mod = null;
        string metadataPath = Path.Combine(path, PackageBuilder.METADATA);

        if (File.Exists(metadataPath)) {
            using FileStream fs = File.OpenRead(metadataPath);
            mod = JsonSerializer.Deserialize<Mod>(fs);
        }

        mod ??= new() {
            Name = Path.GetFileName(path),
            SourceFolder = path
        };

        if (Path.GetFullPath(ModManager.ModsPath) != Path.GetDirectoryName(path)) {
            string output = Path.Combine(ModManager.ModsPath, mod.Id.ToString());
            await PackageBuilder.CopyContents(mod, output);
            PackageBuilder.CreateMetaData(mod, output);
        }

        return mod;
    }

    internal static Mod FromInternal(string path)
    {
        Mod? mod = null;
        string metadataPath = Path.Combine(path, PackageBuilder.METADATA);

        if (File.Exists(metadataPath)) {
            using FileStream fs = File.OpenRead(metadataPath);
            mod = JsonSerializer.Deserialize<Mod>(fs);
        }

        mod ??= new() {
            Name = Path.GetFileName(path),
        };

        mod.SourceFolder = path;
        return mod;
    }
}
