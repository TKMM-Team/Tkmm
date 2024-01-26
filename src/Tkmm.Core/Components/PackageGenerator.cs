using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

public class PackageGenerator
{
    private const string THUMBNAIL_URI = "thumbnail";

    private readonly Mod _mod;
    private readonly string _outputFolder;
    private readonly string _tempRomfsOutput;
    private readonly string _SourceRomfsFolder;

    public PackageGenerator(Mod mod)
    {
        _mod = mod;
        _outputFolder = Path.Combine(Path.GetTempPath(), mod.Id.ToString());
        _tempRomfsOutput = Path.Combine(_outputFolder, "romfs");
        Directory.CreateDirectory(_tempRomfsOutput);
        _SourceRomfsFolder = Path.Combine(_mod.SourceFolder, "romfs");
    }

    public PackageGenerator(Mod mod, string outputFolder)
    {
        _mod = mod;
        _outputFolder = outputFolder;
        _tempRomfsOutput = Path.Combine(_outputFolder, "romfs");
        Directory.CreateDirectory(_tempRomfsOutput);
        _SourceRomfsFolder = Path.Combine(_mod.SourceFolder, "romfs");
    }

    public async Task Build()
    {
        if (Directory.Exists(_outputFolder)) {
            Directory.Delete(_outputFolder, true);
        }

        DirectoryOperations.CopyDirectory(_mod.SourceFolder, _outputFolder, true);

        if (Directory.Exists(Path.Combine(_outputFolder, "GameData")))
        {
            Directory.Delete(Path.Combine(_outputFolder, "GameData"), true);
            Directory.CreateDirectory(Path.Combine(_outputFolder, "GameData"));
            Console.WriteLine("Cleared GameData Folder!");
        }
        else {
            Console.WriteLine("No GameData Folder Found...");
        }

        await ToolHelper.Call("MalsMerger", "gen", _SourceRomfsFolder, _tempRomfsOutput)
            .WaitForExitAsync();

        string rsdbFolderPath = Path.Combine(_mod.SourceFolder, "romfs", "RSDB");

        // Generate changelog for each mod
        await ToolHelper.Call("RsdbMerge",
                "--generate-changelog", rsdbFolderPath,
                "--output", _outputFolder
            ).WaitForExitAsync();

        // Generate changelog for each mod
        await ToolHelper.Call("SarcTool",
                "assemble",
                "--mod", _mod.SourceFolder
            ).WaitForExitAsync();

        // Generate changelog for each mod
        await ToolHelper.Call("SarcTool",
                "package",
                "--mod", _mod.SourceFolder,
                "--output", _outputFolder
            ).WaitForExitAsync();


        if (File.Exists(_mod.ThumbnailUri)) {
            File.Copy(_mod.ThumbnailUri, Path.Combine(_outputFolder, THUMBNAIL_URI), true);
            _mod.ThumbnailUri = THUMBNAIL_URI;
        }

        using FileStream fs = File.Create(Path.Combine(_outputFolder, "info.json"));
        JsonSerializer.Serialize(fs, _mod);
    }

    public void Save(string exportFile, bool clearOutputFolder = false)
    {
        if (Path.GetDirectoryName(exportFile) is string exportFolder) {
            Directory.CreateDirectory(exportFolder);
        }

        using FileStream tkcl = File.Create(exportFile);
        ZipFile.CreateFromDirectory(_outputFolder, tkcl);

        if (clearOutputFolder) {
            Directory.Delete(_outputFolder, true);
        }
    }
}
