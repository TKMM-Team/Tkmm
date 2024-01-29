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

    private static readonly string[] _excludedExtensions = [
        ".rsizetable.zs",
        ".byml.zs",
        ".bgyml",
        ".pack.zs",
        ".sarc.zs",
        ".blarc.zs",
        ".json",
        "thumbnail",
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

        AppStatus.Set("Applying", "fa-solid fa-file-lines", isWorkingStatus: true);
        Apply();

        AppStatus.Set("Merging", "fa-solid fa-code-merge", isWorkingStatus: true);

        if (Directory.Exists(Config.Shared.MergeOutput)) {
            Directory.Delete(Config.Shared.MergeOutput, true);
        }

        Directory.CreateDirectory(Config.Shared.MergeOutput);

        // 
        // Copy loose files in order of priority
        foreach (var mod in Mods.Reverse()) {
            foreach (var file in Directory.EnumerateFiles(mod.SourceFolder, "*.*", SearchOption.AllDirectories)) {
                if (_excludedExtensions.Any(file.EndsWith)) {
                    continue;
                }

                var destinationFile = Path.Combine(Config.Shared.MergeOutput, Path.GetRelativePath(mod.SourceFolder, file));
                if (Path.GetDirectoryName(destinationFile) is string folder) {
                    Directory.CreateDirectory(folder);
                }

                File.Copy(file, destinationFile, true);
            }

            string exefsPath = Path.Combine(mod.SourceFolder, EXEFS);
            if (Directory.Exists(exefsPath)) {
                DirectoryOperations.CopyDirectory(exefsPath, Path.Combine(Config.Shared.MergeOutput, EXEFS), true);
            }
        }

        // 
        // Merge Mals archives
        await ToolHelper.Call(Tool.MalsMerger,
                "merge", string.Join('|', Mods.Select(x => Path.Combine(x.SourceFolder, "romfs"))),
                "--target", Config.Shared.GameLanguage,
                Path.Combine(Config.Shared.MergeOutput, "romfs")
            ).WaitForExitAsync();

        // Merge Sarc and BYML
        await ToolHelper.Call(Tool.SarcTool, [
                "merge",
                "--base", ModsPath,
                "--mods", .. Mods.Select(x => x.Id.ToString()),
                "--process", "All",
                "--output", Path.Combine(Config.Shared.MergeOutput, "romfs")
            ]).WaitForExitAsync();

        // Merge RSDB
        string outputRsdbFolder = Path.Combine(Config.Shared.MergeOutput, "romfs", "RSDB");
        Directory.CreateDirectory(outputRsdbFolder);

        await ToolHelper.Call(Tool.RsdbMerger,
                "--apply-changelogs", string.Join('|', Mods.Select(x => x.SourceFolder)),
                "--output", outputRsdbFolder,
                "--version", TotkConfig.Shared.Version.ToString()
            ).WaitForExitAsync();

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
}
