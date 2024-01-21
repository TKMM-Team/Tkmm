using System.IO.Compression;

namespace Tkmm.Core.Components.Models;

public class ArchiveModImporter(ZipArchive archive) : IModImporter
{
    private readonly ZipArchive _archive = archive;

    public void Import(string importPath)
    {
        if (Directory.Exists(importPath)) {
            Directory.Delete(importPath, true);
        }

        _archive.ExtractToDirectory(importPath);
        _archive.Dispose();
    }
}
