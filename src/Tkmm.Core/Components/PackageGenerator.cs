using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

public class PackageGenerator
{
    private const string THUMBNAIL_URI = "thumbnail";

    private readonly Mod _mod;
    private readonly string _outputFolder;

    public PackageGenerator(Mod mod)
    {
        _mod = mod;
        _outputFolder = Path.Combine(Path.GetTempPath(), mod.Id.ToString());
    }

    public PackageGenerator(Mod mod, string outputFolder)
    {
        _mod = mod;
        _outputFolder = outputFolder;
    }

    public async Task Build()
    {
        AppStatus.Set("Building package", "fa-solid fa-boxes-packing");
        await Build(_mod, _mod.SourceFolder, _outputFolder);

        foreach (var group in _mod.OptionGroups) {
            string groupOutputFolder = Path.Combine(_outputFolder, "options", group.Id.ToString());
            await Build(group, group.SourceFolder, groupOutputFolder);

            foreach (var option in group.Options) {
                await Build(option, option.SourceFolder, Path.Combine(groupOutputFolder, option.Id.ToString()));
            }
        }

        AppStatus.Set("Package built", "fa-solid fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
    }

    private async Task Build<T>(T modItem, string sourceFolder, string outputFolder) where T : IModItem
    {
        if (Directory.Exists(outputFolder)) {
            Directory.Delete(outputFolder, true);
        }

        // TODO: Certain files should
        // be excluded from this
        DirectoryOperations.CopyDirectory(sourceFolder, outputFolder, true);

        string romfsOutput = Path.Combine(outputFolder, "romfs");
        Directory.CreateDirectory(romfsOutput);

        await ToolHelper.Call("MalsMerger", "gen", Path.Combine(sourceFolder, "romfs"), romfsOutput)
            .WaitForExitAsync();

        string rsdbFolderPath = Path.Combine(sourceFolder, "romfs", "RSDB");

        // Generate changelog for each mod
        await ToolHelper.Call("RsdbMerge",
                "--generate-changelog", rsdbFolderPath,
                "--output", _outputFolder
            ).WaitForExitAsync();

        // Generate changelog for each mod
        await ToolHelper.Call("SarcTool",
                "package",
                "--mod", sourceFolder,
                "--output", _outputFolder
            ).WaitForExitAsync();

        if (File.Exists(modItem.ThumbnailUri)) {
            File.Copy(modItem.ThumbnailUri, Path.Combine(outputFolder, THUMBNAIL_URI), true);
            modItem.ThumbnailUri = THUMBNAIL_URI;
        }

        using FileStream fs = File.Create(Path.Combine(outputFolder, "info.json"));
        JsonSerializer.Serialize(fs, modItem);
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
