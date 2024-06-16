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

    public async Task<Mod> Read(Stream? _, string path, Guid? modId)
    {
        Mod? mod = null;
        string metadataPath = Path.Combine(path, PackageBuilder.METADATA);

        if (File.Exists(metadataPath)) {
            using FileStream fs = File.OpenRead(metadataPath);
            mod = JsonSerializer.Deserialize<Mod>(fs);
        }

        mod ??= new() {
            Id = modId ?? Guid.NewGuid(),
            Name = Path.GetFileName(path)
        };

        mod.RefreshOptions(path);

        if (Path.GetDirectoryName(path) != ProfileManager.ModsFolder) {
            string output = ProfileManager.GetModFolder(mod);
            await PackageBuilder.CopyContents(mod, path, output);
            PackageBuilder.CreateMetaData(mod, output);
        }

        return mod;
    }

    public static Mod? FromInternal(string path)
    {
        Mod? mod = null;
        string metadataPath = Path.Combine(path, PackageBuilder.METADATA);

        if (File.Exists(metadataPath)) {
            using FileStream fs = File.OpenRead(metadataPath);
            mod = JsonSerializer.Deserialize<Mod>(fs);
        }

        mod?.RefreshOptions();
        return mod;
    }
}
