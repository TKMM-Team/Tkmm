using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System.Diagnostics;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.ModReaders;

public class SevenZipModReader : IModReader
{
    public bool IsValid(string file)
    {
        return Path.GetExtension(file) == ".7z";
    }

    public async Task<Mod> Read(Stream? input, string file, Guid? modId)
    {
        ArgumentNullException.ThrowIfNull(input);

        string fileName = Path.GetFileNameWithoutExtension(file);

        Guid id = modId ?? Guid.NewGuid();
        string tmpOutputFolder = Path.Combine(Path.GetTempPath(), "tkmm", id.ToString());
        string? root = null;

        if (Config.Shared.SevenZipPath is string sevenZip && File.Exists(sevenZip)) {
            try {
                if (!File.Exists(file)) {
                    file = Path.Combine(tmpOutputFolder, Guid.NewGuid().ToString());
                    using FileStream fs = File.Create(tmpOutputFolder);
                    await input.CopyToAsync(fs);
                }

                await Process.Start(sevenZip, ["x", $"-o{tmpOutputFolder}", "-mtm-", file])
                    .WaitForExitAsync();

                if (DirectoryOperations.LocateTargets(tmpOutputFolder, ArchiveModReader.Targets) is not string targetsParent) {
                    goto Invalid;
                }

                DirectoryOperations.ClearAttributes(tmpOutputFolder);
                Mod result = await Mod.FromPath(targetsParent);
                Directory.Delete(tmpOutputFolder, recursive: true);
                return result;
            }
            catch (Exception ex) {
                AppLog.Log("Failed to extract archive using the configured 7z executable.", LogLevel.Error);
                AppLog.Log(ex);
            }
        }

        using (MemoryStream ms = new()) {
            input.CopyTo(ms);

            using SevenZipArchive archive = SevenZipArchive.Open(ms);

            foreach (var entry in archive.Entries) {
                if (ArchiveModReader.ProcessArchiveEntry(entry, ref root) && root is not null && entry.Key is not null) {
                    string output = Path.Combine(tmpOutputFolder, Path.GetRelativePath(root, entry.Key.Trim('/', '\\')));
                    if (Path.GetDirectoryName(output) is string absoluteOutputFolder) {
                        Directory.CreateDirectory(absoluteOutputFolder);
                    }

                    entry.WriteToFile(output, new ExtractionOptions {
                        PreserveAttributes = false,
                        PreserveFileTime = true,
                        ExtractFullPath = false,
                        Overwrite = true
                    });
                }
            }
        }

    Invalid:
        if (root is null) {
            throw new InvalidOperationException("""
                Invalid mod archive, a valid folder could not be found.
                """);
        }

        Mod mod = new() {
            Id = id,
            Name = fileName
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
}
