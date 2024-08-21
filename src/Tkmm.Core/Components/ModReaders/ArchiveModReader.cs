using SharpCompress.Common;
using SharpCompress.Readers;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModReaders;

public class ArchiveModReader : IModReader
{
    public static readonly string[] Targets = [
        PackageBuilder.OPTIONS,
        TotkConfig.ROMFS,
        TotkConfig.EXEFS,
        TotkConfig.CHEATS,
    ];

    public bool IsValid(string file)
    {
        return Path.GetExtension(file) is ".rar" or ".zip";
    }

    public async Task<Mod> Read(Stream? input, string file, Guid? modId)
    {
        ArgumentNullException.ThrowIfNull(input);

        using IReader reader = ReaderFactory.Open(input);

        Guid id = modId ?? Guid.NewGuid();
        string tmpOutputFolder = Path.Combine(Path.GetTempPath(), "tkmm", id.ToString());
        string? root = null;

        while (reader.MoveToNextEntry()) {
            IEntry entry = reader.Entry;
            if (ProcessArchiveEntry(entry, ref root) && root is not null && entry.Key is not null) {
                string output = Path.Combine(tmpOutputFolder, entry.Key[root.Length..].Trim('/', '\\'));
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
            Name = Path.GetFileNameWithoutExtension(file)
        };

        string outputFolder = ProfileManager.GetModFolder(id);

        try {
            PackageBuilder.CreateMetaData(mod, outputFolder);
            await PackageBuilder.CopyContents(mod, tmpOutputFolder, outputFolder);
        }
        finally {
            Directory.Delete(tmpOutputFolder, true);
        }

        return mod;
    }

    internal static bool ProcessArchiveEntry(IEntry entry, ref string? root)
    {
        ArgumentNullException.ThrowIfNull(entry.Key);

        if (root is null) {
            foreach (var folder in Targets) {
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
