using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

public class PackageBuilder
{
    public const string METADATA = "info.json";
    public const string THUMBNAIL = "thumbnail";
    public const string OPTIONS = "options";

    private const string CHECK_ICON = "fa-solid fa-circle-check";
    private const string METADATA_ICON = "fa-regular fa-file-code";
    private const string COPY_ICON = "fa-solid fa-object-group";
    private const string PACKAGE_ICON = "fa-solid fa-boxes-packing";

    private static readonly string[] _fileSystemFolders = [
        ModManager.ROMFS,
        ModManager.EXEFS
    ];

    public static void CreateMetaData<T>(T item, string outputFolder) where T : IModItem
    {
        AppStatus.Set($"Creating metadata for '{item.Name}'", METADATA_ICON);
        Directory.CreateDirectory(outputFolder);

        string metadataFile = Path.Combine(outputFolder, METADATA);
        using FileStream fs = File.Create(metadataFile);
        JsonSerializer.Serialize(fs, item);

        if (File.Exists(item.ThumbnailUri)) {
            AppStatus.Set($"Copying thumbnail", METADATA_ICON);
            File.Copy(item.ThumbnailUri, Path.Combine(outputFolder, THUMBNAIL), true);
            item.ThumbnailUri = THUMBNAIL;
        }

        if (item is Mod mod) {
            foreach (var group in mod.OptionGroups) {
                string groupOutputFolder = Path.Combine(outputFolder, OPTIONS, group.Id.ToString());
                CreateMetaData(group, groupOutputFolder);

                foreach (var option in group.Options) {
                    string optionOutputFolder = Path.Combine(groupOutputFolder, option.Id.ToString());
                    CreateMetaData(option, optionOutputFolder);
                }
            }
            
            AppStatus.Set($"Metadata generated for mod '{item.Name}'", CHECK_ICON);
        }
    }

    public static async Task CopyContents<T>(T item, string outputFolder) where T : IModItem
    {
        AppStatus.Set($"Copying '{item.Name}'", COPY_ICON);

        AppStatus.Set("Generating Mals", COPY_ICON);
        ToolHelper.Call(Tool.MalsMerger,
                "gen", item.SourceFolder,
                Path.Combine(outputFolder, "romfs")
            ).WaitForExit();

        AppStatus.Set("Generating RSDB", COPY_ICON);
        ToolHelper.Call(Tool.RsdbMerger,
                "--generate-changelog", Path.Combine(item.SourceFolder, "romfs", "RSDB"),
                "--output", outputFolder
            ).WaitForExit();

        AppStatus.Set("Assembling SARC", COPY_ICON);
        ToolHelper.Call(Tool.SarcTool,
                "assemble",
                "--mod", item.SourceFolder
            ).WaitForExit();

        AppStatus.Set("Packing SARC", COPY_ICON);
        ToolHelper.Call(Tool.SarcTool,
                "package",
                "--mod", item.SourceFolder,
                "--output", outputFolder
            ).WaitForExit();

        AppStatus.Set("Copying file-system folders", COPY_ICON);
        foreach (var folder in _fileSystemFolders) {
            string inputFsFolder = Path.Combine(item.SourceFolder, folder);

            if (Directory.Exists(inputFsFolder)) {
                DirectoryOperations.CopyDirectory(
                    inputFsFolder, Path.Combine(outputFolder, folder),
                    ToolHelper.ExcludeFiles, ToolHelper.ExcludeFolders,
                    overwrite: true
                );
            }
        }

        if (item is Mod mod) {
            AppStatus.Set("Copying options", COPY_ICON);

            foreach (var group in mod.OptionGroups) {
                string groupOutputFolder = Path.Combine(outputFolder, OPTIONS, group.Id.ToString());
                
                foreach (var option in group.Options) {
                    string optionOutputFolder = Path.Combine(groupOutputFolder, option.Id.ToString());
                    CopyContents(option, optionOutputFolder);
                }
            }
        }

        AppStatus.Set($"Copied '{item.Name}'", CHECK_ICON, isWorkingStatus: false, temporaryStatusTime: 1.5);
    }

    public static void Package(string inputFolder, string outputFile)
    {
        using FileStream fs = File.Create(outputFile);
        ZipFile.CreateFromDirectory(inputFolder, fs);
    }
}
