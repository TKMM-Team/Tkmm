using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System.Buffers;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModParsers;

public class SevenZipModReader : IModReader
{
    private static readonly string[] _folders = [
        PackageBuilder.OPTIONS,
        ModManager.ROMFS,
        ModManager.EXEFS
    ];

    public bool IsValid(string file)
    {
        return Path.GetExtension(file) == ".7z";
    }

    public Mod Parse(Stream input, string file)
    {
        using MemoryStream ms = new();
        input.CopyTo(ms);

        using SevenZipArchive archive = SevenZipArchive.Open(ms);

        Guid id = Guid.NewGuid();
        string outputFolder = Path.Combine(ModManager.ModsPath, id.ToString());
        string? root = null;

        foreach (var entry in archive.Entries) {
            if (ArchiveModReader.ProcessArchiveEntry(entry, ref root) && root is not null) {
                string output = Path.Combine(outputFolder, Path.GetRelativePath(root, entry.Key.Trim('/', '\\')));
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
            Name = Path.GetFileNameWithoutExtension(file),
            SourceFolder = outputFolder,
        };

        return mod;
    }
}
