using System.Text.Json;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModReaders;

public class FolderModReader : IModReader
{
    public bool IsValid(string path)
    {
        return Directory.Exists(path) && (
            Directory.Exists(Path.Combine(path, TotkConfig.ROMFS)) || Directory.Exists(Path.Combine(path, TotkConfig.EXEFS))
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

        if (Path.GetFullPath(ProfileManager.ModsFolder) != Path.GetDirectoryName(path)) {
            string output = ProfileManager.GetModFolder(mod);
            await PackageBuilder.CopyContents(mod, output);
            PackageBuilder.CreateMetaData(mod, output);
        }

        return mod;
    }

    internal static Mod? FromInternal(string path)
    {
        Mod? mod = null;
        string metadataPath = Path.Combine(path, PackageBuilder.METADATA);

        if (File.Exists(metadataPath)) {
            using FileStream fs = File.OpenRead(metadataPath);
            mod = JsonSerializer.Deserialize<Mod>(fs);
        }

        if (mod is not null) {
            mod.SourceFolder = path;
        }

        return mod;
    }
}
