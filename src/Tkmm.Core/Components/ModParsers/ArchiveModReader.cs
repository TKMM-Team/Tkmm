using SharpCompress.Common;
using SharpCompress.Readers;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModParsers;

public class ArchiveModReader : IModReader
{
    private static readonly string[] _folders = [
        PackageBuilder.OPTIONS,
        ModManager.ROMFS,
        ModManager.EXEFS
    ];

    public bool IsValid(string file)
    {
        return Path.GetExtension(file) is ".rar" or ".zip";
    }

    public Task<Mod> Read(Stream? input, string file)
    {
        ArgumentNullException.ThrowIfNull(input);

        using IReader reader = ReaderFactory.Open(input);

        Guid id = Guid.NewGuid();
        string outputFolder = Path.Combine(ModManager.ModsPath, id.ToString());
        string? root = null;

        while (reader.MoveToNextEntry()) {
            IEntry entry = reader.Entry;
            if (ProcessArchiveEntry(entry, ref root) && root is not null) {
                string output = Path.Combine(outputFolder, Path.GetRelativePath(root, entry.Key.Trim('/', '\\')));
                if (Path.GetDirectoryName(output) is string absoluteOutputFolder) {
                    Directory.CreateDirectory(absoluteOutputFolder);
                }

                reader.WriteEntryToFile(output, new ExtractionOptions {
                    PreserveAttributes = false,
                    PreserveFileTime = true,
                    ExtractFullPath = false,
                    Overwrite = true
                });
            }
        }

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

        PackageBuilder.CreateMetaData(mod, outputFolder);
        return Task.FromResult(mod);
    }

    internal static bool ProcessArchiveEntry(IEntry entry, ref string? root)
    {
        if (root is null) {
            foreach (var folder in _folders) {
                int index = entry.Key.IndexOf(folder);
                if (index > -1) {
                    root = entry.Key[..index].Trim('/', '\\');
                    break;
                }
            }
        }

        if (root is null || entry.IsDirectory || !entry.Key.StartsWith(root)) {
            return false;
        }

        string fileName = entry.Key.Trim('/', '\\');
        if (fileName.EndsWith(".rsizetable.zs")) {
            // Skip RSTB files for speed
            return false;
        }

        return true;
    }
}
