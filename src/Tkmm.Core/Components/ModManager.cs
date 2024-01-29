using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Tkmm.Core.Components.ModParsers;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

/// <summary>
/// This class will handle all of the
/// operations regarding loading the mod list
/// </summary>
public partial class ModManager : ObservableObject
{
    public const string ROMFS = "romfs";
    public const string EXEFS = "exefs";

    public static readonly string ModsPath = Path.Combine(Config.Shared.StorageFolder, "mods");

    public static readonly string[] FileSystemFolders = [
        ROMFS,
        EXEFS
    ];

    private static List<string> ExcludeFiles => [
        .. ToolHelper.ExcludeFiles,
        ".json",
        ".thumbnail"
    ];

    private static readonly Lazy<ModManager> _shared = new(() => new());
    public static ModManager Shared => _shared.Value;

    [ObservableProperty]
    private ObservableCollection<Mod> _mods = [];

    public ModManager()
    {
        string modList = Path.Combine(Config.Shared.StorageFolder, "mods.json");
        if (!File.Exists(modList)) {
            return;
        }

        using FileStream fs = File.OpenRead(modList);
        List<string> mods = JsonSerializer.Deserialize<List<string>>(fs)
            ?? [];

        foreach (string mod in mods) {
            string modFolder = Path.Combine(Config.Shared.StorageFolder, "mods", mod);
            if (Directory.Exists(modFolder)) {
                Mods.Add(FolderModReader.FromInternal(modFolder));
            }
        }
    }

    public static string GetModFolder(Mod mod)
    {
        return Path.Combine(ModsPath, mod.Id.ToString());
    }

    /// <summary>
    /// Import a mod from a .tkcl file or folder
    /// </summary>
    /// <returns></returns>
    public async Task<Mod> Import(string path)
    {
        Mod mod = await Mod.FromPath(path);
        Mods.Add(mod);
        return mod;
    }

    /// <summary>
    /// Apply the load order and save the current profile
    /// </summary>
    /// <returns></returns>
    public void Apply()
    {
        string modList = Path.Combine(Config.Shared.StorageFolder, "mods.json");
        using FileStream fs = File.Create(modList);
        JsonSerializer.Serialize(fs, Mods.Select(x => x.Id));

        AppStatus.Set("Saved mods profile!", "fa-solid fa-list-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
    }

    /// <summary>
    /// Merge all mods :D
    /// </summary>
    /// <returns></returns>
    public async Task Merge()
    {
        // If there are no mods, skip merging
        if (Mods.Count <= 0) {
            AppStatus.Set("Nothing to Merge", "fa-solid fa-code-merge", isWorkingStatus: false, temporaryStatusTime: 1.5);
            Trace.WriteLine("[Info] No mods to merge!");
        }

        AppStatus.Set("Applying", "fa-solid fa-file-lines");
        Apply();

        AppStatus.Set("Merging", "fa-solid fa-code-merge");

        if (Directory.Exists(Config.Shared.MergeOutput)) {
            Directory.Delete(Config.Shared.MergeOutput, true);
        }

        Directory.CreateDirectory(Config.Shared.MergeOutput);

        Task[] tasks = [
            Task.Run(CopyContents),

            // Mals
            ToolHelper.Call(Tool.MalsMerger,
                "merge", string.Join('|', Mods.Select(x => Path.Combine(x.SourceFolder, "romfs"))),
                Path.Combine(Config.Shared.MergeOutput, "romfs"),
                "--target", Config.Shared.GameLanguage
            ).WaitForExitAsync(),

            // Sarc
            ToolHelper.Call(Tool.SarcTool, [
                "merge",
                "--base", ModsPath,
                "--mods", .. Mods.Select(x => x.Id.ToString()),
                "--process", "All",
                "--output", Path.Combine(Config.Shared.MergeOutput, "romfs")
            ]).WaitForExitAsync(),

            // RSDB
            ToolHelper.Call(Tool.RsdbMerger,
                "--apply-changelogs", string.Join('|', Mods.Select(x => x.SourceFolder)),
                "--output", Path.Combine(Config.Shared.MergeOutput, "romfs", "RSDB"),
                "--version", TotkConfig.Shared.Version.ToString()
            ).WaitForExitAsync()
        ];

        await Task.WhenAll(tasks);

        // After merging, execute Restbl on the merged mod folder
        await ToolHelper.Call(Tool.RestblMerger,
                "--action", "single-mod",
                "--use-checksums",
                "--version", TotkConfig.Shared.Version.ToString(),
                "--mod-path", Config.Shared.MergeOutput,
                "--compress"
            ).WaitForExitAsync();

        AppStatus.Set("Merge Completed", "fa-solid fa-list-check", isWorkingStatus: false, 1.5);
        Trace.WriteLine("[Info] Merge completed successfully");
    }

    private void CopyContents()
    {
        foreach (var mod in Mods.Reverse()) {
            CopyContents(mod.SourceFolder);
            foreach (var group in mod.OptionGroups.Reverse()) {
                foreach (var option in group.Options) {
                    CopyContents(option.SourceFolder);
                }
            }
        }
    }

    private static void CopyContents(string sourceFolder)
    {
        foreach (var folder in FileSystemFolders) {
            string srcFolder = Path.Combine(sourceFolder, folder);
            if (Directory.Exists(srcFolder)) {
                DirectoryOperations.CopyDirectory(srcFolder, Path.Combine(Config.Shared.MergeOutput, folder), ExcludeFiles, ToolHelper.ExcludeFolders, true);
            }
        }
    }
}
