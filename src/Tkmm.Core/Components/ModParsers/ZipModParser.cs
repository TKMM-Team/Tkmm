using System.IO.Compression;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModParsers;

public class ZipModParser : IModParser
{
    private static readonly string[] _validFolders = [
        ModManager.ROMFS,
        ModManager.EXEFS,
        PackageBuilder.OPTIONS
    ];

    public bool IsValid(string file)
    {
        return Path.GetExtension(file) == ".zip";
    }

    public Mod Parse(Stream input, string file)
    {
        ZipArchive archive = new(input);

        Guid id = Guid.NewGuid();
        string outputFolder = Path.Combine(ModManager.ModsPath, id.ToString());

        bool isValidArchive = false;
        foreach (var entry in archive.Entries) {
            string name = Path.GetFileName(entry.FullName.Trim('/'));

            // TODO: Don't copy options twice
            if (_validFolders.Contains(name)) {
                isValidArchive = true;
                ExtractEntry(entry, outputFolder, name);
            }
        }

        if (!isValidArchive) {
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

    private static void ExtractEntry(ZipArchiveEntry target, string outputFolder, string name)
    {
        string output = Path.Combine(outputFolder, name);
        foreach (var entry in target.Archive.Entries.Where(x => x.FullName.StartsWith(target.FullName))) {
            if (string.IsNullOrEmpty(entry.Name)) {
                // Skip folders
                continue;
            }

            string outputFile = Path.Combine(outputFolder, Path.GetRelativePath(target.FullName, entry.FullName));
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile) ?? string.Empty);
            entry.ExtractToFile(outputFile);
        }
    }
}
