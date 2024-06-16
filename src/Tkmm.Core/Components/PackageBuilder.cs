using CommunityToolkit.HighPerformance;
using MalsMerger.Core;
using RsdbMerger.Core.Services;
using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core.Components.ModReaders;
using Tkmm.Core.Exceptions;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;
using TKMM.SarcTool.Core;

namespace Tkmm.Core.Components;

public class PackageBuilder
{
    public const string METADATA = "info.json";
    public const string THUMBNAIL = ".thumbnail";
    public const string OPTIONS = "options";

    private const string CHECK_ICON = "fa-solid fa-circle-check";
    private const string METADATA_ICON = "fa-regular fa-file-code";
    private const string COPY_ICON = "fa-solid fa-object-group";

    public static void CreateMetaData<T>(T item, string outputFolder, bool useSourceFolderName = false) where T : IModItem
    {
        AppStatus.Set($"Creating metadata for '{item.Name}'", METADATA_ICON);
        Directory.CreateDirectory(outputFolder);

        string? tempThumbnailUri = item.ThumbnailUri;
        if (File.Exists(item.ThumbnailUri)) {
            AppStatus.Set($"Copying thumbnail", METADATA_ICON);
            File.Copy(item.ThumbnailUri, Path.Combine(outputFolder, THUMBNAIL), true);
            item.ThumbnailUri = THUMBNAIL;
        }

        string metadataFile = Path.Combine(outputFolder, METADATA);
        using FileStream fs = File.Create(metadataFile);
        JsonSerializer.Serialize(fs, item);

        item.ThumbnailUri = tempThumbnailUri;

        if (item is Mod mod) {
            foreach (var group in mod.OptionGroups) {
                string groupOutputFolder = useSourceFolderName
                    ? Path.Combine(outputFolder, OPTIONS, Path.GetFileName(group.SourceFolder))
                    : Path.Combine(outputFolder, OPTIONS, group.Id.ToString());
                CreateMetaData(group, groupOutputFolder);

                foreach (var option in group.Options) {
                    string optionOutputFolder = useSourceFolderName
                        ? Path.Combine(groupOutputFolder, Path.GetFileName(option.SourceFolder))
                        : Path.Combine(groupOutputFolder, option.Id.ToString());
                    CreateMetaData(option, optionOutputFolder);
                }
            }

            AppStatus.Set($"Metadata generated for mod '{item.Name}'", CHECK_ICON, isWorkingStatus: false, temporaryStatusTime: 1.5);
        }
    }

    public static async Task CopyContents<T>(T item, string sourceFolder, string outputFolder) where T : IModItem
    {
        try {
            await Task.WhenAll(CopyContentsInternal(item, sourceFolder, outputFolder));
            AppStatus.Set($"Packaged '{item.Name}'", CHECK_ICON, isWorkingStatus: false, temporaryStatusTime: 1.5);
        }
        catch (Exception ex) {
            throw new PackageException(item, outputFolder, ex);
        }
    }

    private static List<Task> CopyContentsInternal<T>(T item, string sourceFolder, string outputFolder) where T : IModItem
    {
        AppStatus.Set($"Generating changelogs for '{item.Name}'", COPY_ICON);

        string inputRomfs = Path.Combine(sourceFolder, TotkConfig.ROMFS);
        string outputRomfs = Path.Combine(outputFolder, TotkConfig.ROMFS);

        List<Task> tasks = [

            // Mals
            Task.Run(() => {
                if (Directory.Exists(Path.Combine(sourceFolder, TotkConfig.ROMFS, "Mals"))) {
                    Merger malsMerger = new([inputRomfs], outputRomfs, null);
                    malsMerger.GenerateChangelogs(format: false);
                }
            }),

            // RSDB
            Task.Run(async () => {
                RsdbChangelogService changelogService = new(inputRomfs, outputRomfs);
                await changelogService.CreateChangelogsAsync();
            }),

            // SARC
            Task.Run(() => {
                SarcAssembler assembler = new(Path.Combine(sourceFolder, "romfs"));
                assembler.Assemble();
            
                SarcPackager packager = new(Path.Combine(outputFolder, "romfs"), Path.Combine(sourceFolder, "romfs"));
                packager.Package();
            }),

            // General
            Task.Run(() => {
                foreach (var folder in TotkConfig.FileSystemFolders) {
                    string inputFsFolder = Path.Combine(sourceFolder, folder);

                    if (Directory.Exists(inputFsFolder)) {
                        DirectoryOperations.CopyDirectory(
                            inputFsFolder, Path.Combine(outputFolder, folder),
                            [..ExcludeInfo.Extensions], [..ExcludeInfo.Folders],
                            overwrite: true
                        );
                    }
                }
            })
        ];

        if (item is Mod mod) {
            AppStatus.Set("Copying options", COPY_ICON);

            foreach (var group in mod.OptionGroups) {
                string groupOutputFolder = Path.Combine(outputFolder, OPTIONS, group.Id.ToString());

                foreach (var option in group.Options) {
                    string optionOutputFolder = Path.Combine(groupOutputFolder, option.Id.ToString());
                    tasks.AddRange(CopyContentsInternal(option, option.SourceFolder, optionOutputFolder));
                }
            }
        }

        AppStatus.Set("Waiting for build processes", COPY_ICON);

        return tasks;
    }

    public static void Package(string inputFolder, string outputFile)
    {
        using FileStream fs = File.Create(outputFile);
        fs.Write(TkclModReader.MAGIC);
        fs.Write(TkclModReader._version);
        ZipFile.CreateFromDirectory(inputFolder, fs);
    }
}
