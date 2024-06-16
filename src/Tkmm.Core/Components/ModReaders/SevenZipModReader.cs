using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModReaders;

public class SevenZipModReader : IModReader
{
    public bool IsValid(string file)
    {
        return Path.GetExtension(file) == ".7z";
    }

    public async Task<Mod> Read(Stream? input, string file)
    {
        ArgumentNullException.ThrowIfNull(input);

        using MemoryStream ms = new();
        input.CopyTo(ms);

        using SevenZipArchive archive = SevenZipArchive.Open(ms);

        Guid id = Guid.NewGuid();
        string tmpOutputFolder = Path.Combine(Path.GetTempPath(), "tkmm", id.ToString());
        string? root = null;

        foreach (var entry in archive.Entries) {
            if (ArchiveModReader.ProcessArchiveEntry(entry, ref root) && root is not null && entry.Key is not null) {
                string output = Path.Combine(tmpOutputFolder, Path.GetRelativePath(root, entry.Key.Trim('/', '\\')));
                if (Path.GetDirectoryName(output) is string absoluteOutputFolder) {
                    Directory.CreateDirectory(absoluteOutputFolder);
                }

                entry.WriteToFile(output, new ExtractionOptions {
                    PreserveAttributes = true,
                    PreserveFileTime = true,
                    ExtractFullPath = false,
                    Overwrite = true
                });
            }
        }

        archive.Dispose();

        if (root is null) {
            throw new InvalidOperationException("""
                Invalid mod archive, a valid folder could not be found.
                """);
        }

        Mod mod = new() {
            Id = id,
            Name = Path.GetFileNameWithoutExtension(file)
        };

        string outputFolder = ProfileManager.GetModFolder(id);

        PackageBuilder.CreateMetaData(mod, outputFolder);
        await PackageBuilder.CopyContents(mod, tmpOutputFolder, outputFolder);
        Directory.Delete(tmpOutputFolder, true);

        return mod;
    }
}
