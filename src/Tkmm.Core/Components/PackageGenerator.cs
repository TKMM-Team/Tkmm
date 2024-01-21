using System.IO.Compression;
using System;
using System.IO;
using System.Text.Json;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

public class PackageGenerator
{
    private readonly Mod _mod;
    private readonly string _tempOutput;
    private readonly string _tempRomfsOutput;
    private readonly string _exportFileLocation;
    private readonly string _SourceRomfsFolder;

    public PackageGenerator(Mod mod, string tkclExportFileLocation)
    {
        _mod = mod;
        _exportFileLocation = tkclExportFileLocation;
        _tempOutput = Path.Combine(Path.GetTempPath(), mod.Id.ToString());
        _tempRomfsOutput = Path.Combine(_tempOutput, "romfs");
        Directory.CreateDirectory(_tempRomfsOutput);
        _SourceRomfsFolder = Path.Combine(_mod.SourceFolder, "romfs");
    }

    public async Task Build()
    {
        if (Directory.Exists(_tempOutput))
        {
            Directory.Delete(_tempOutput, true);
        }
        Directory.CreateDirectory(_tempOutput);

        // Define the file extensions and subfolders to exclude
        var excludedExtensions = new HashSet<string> { ".rsizetable.zs", ".byml.zs", ".bgyml", ".pack.zs", ".sarc.zs", ".blarc.zs" };

        string exefsPath = Path.Combine(_mod.SourceFolder, "exefs");
        string destinationDir = Path.Combine(_tempOutput, "exefs");

        if (Directory.Exists(exefsPath))

        {
            Directory.CreateDirectory(destinationDir);


            foreach (var file in Directory.EnumerateFiles(exefsPath, "*.*", SearchOption.AllDirectories))
            {
                // Calculate the relative path
                string relativePath = file.Substring(exefsPath.Length + 1);

                // Construct the destination file path
                string destFile = Path.Combine(destinationDir, relativePath);

                // Create the directory if it doesn't exist
                string destDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                // Copy the file
                File.Copy(file, destFile, true);
            }
        }
        // Enumerate all files in the source folder and its subfolders
        foreach (var file in Directory.EnumerateFiles(_mod.SourceFolder, "*.*", SearchOption.AllDirectories))
        {   
            var fileInfo = new FileInfo(file);

            // Compute the relative directory path

            // Skip the file if its name ends with any of the excluded extensions
            if (excludedExtensions.Any(ex => fileInfo.Name.EndsWith(ex)))
                continue;

            // Compute the destination path
            var relativePath = fileInfo.FullName.Substring(_mod.SourceFolder.Length + 1);
            var destinationPath = Path.Combine(_tempOutput, relativePath);

            // Create the destination directory if it doesn't exist
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            // Copy the file
            File.Copy(file, destinationPath, true);
        }

        await ToolHelper.Call("MalsMerger", "gen", _SourceRomfsFolder, _tempRomfsOutput)
            .WaitForExitAsync();

        string rsdbFolderPath = Path.Combine(_mod.SourceFolder, "romfs", "RSDB");

        // Generate changelog for each mod
        await ToolHelper.Call("RsdbMerge",
            "--generate-changelog", rsdbFolderPath,
            "--output", _tempOutput)
                .WaitForExitAsync();

        // Generate changelog for each mod
        await ToolHelper.Call("SarcTool",
            "package", 
            "--mod", _mod.SourceFolder,
            "--output", _tempOutput)
                .WaitForExitAsync();

        if (File.Exists(Path.Combine(_tempOutput, "thumbnail")))
        {
            File.Delete(Path.Combine(_tempOutput, "thumbnail"));
        }

        if (File.Exists(_mod.ThumbnailUri)) {
            File.Copy(_mod.ThumbnailUri, Path.Combine(_tempOutput, "thumbnail"));
            _mod.ThumbnailUri = "thumbnail";
        }

        using (FileStream fs = File.Create(Path.Combine(_tempOutput, "info.json"))) {
            JsonSerializer.Serialize(fs, _mod);
        }

        if (Path.GetDirectoryName(_exportFileLocation) is string exportFolder) {
            Directory.CreateDirectory(exportFolder);
        }

        using FileStream tkcl = File.Create(_exportFileLocation);
        ZipFile.CreateFromDirectory(_tempOutput, tkcl);

        Directory.Delete(_tempOutput, true);
    }
}
