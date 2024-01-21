using System.Diagnostics;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Components.Models;

public class FolderModImporter(string sourceFolder) : IModImporter
{
    private readonly string _sourceFolder = sourceFolder;

    public void Import(string importPath)
    {
        if (Directory.Exists(importPath)) {
            Directory.Delete(importPath, true);
        }

        DirectoryOperations.CopyDirectory(_sourceFolder, importPath, overwrite: true);
    }

}
