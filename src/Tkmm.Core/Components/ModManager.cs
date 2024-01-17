using CommunityToolkit.Mvvm.ComponentModel;
using Octokit;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

/// <summary>
/// This class will handle all of the
/// operations regarding loading the mod list
/// </summary>
public partial class ModManager : ObservableObject
{
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
                Mods.Add(Mod.FromFolder(modFolder, isFromStorage: true));
            }
        }
    }

    /// <summary>
    /// Import a mod from a tcl file or folder
    /// </summary>
    /// <returns></returns>
    public Mod Import(string path)
    {
        Mod mod = File.Exists(path)
            ? Mod.FromFile(path) : Mod.FromFolder(path);

        // If any mods exists with the id
        // stage it to be imported again
        if (Mods.FirstOrDefault(x => x.Id == mod.Id) is Mod existing) {
            existing.StageImport(path);
            return existing;
        }

        Mods.Add(mod);
        return mod;
    }

    /// <summary>
    /// Apply the load order and save the current profile
    /// </summary>
    /// <returns></returns>
    public void Apply()
    {
        foreach (var mod in Mods) {
            mod.Import();
        }

        string modList = Path.Combine(Config.Shared.StorageFolder, "mods.json");
        using FileStream fs = File.Create(modList);
        JsonSerializer.Serialize(fs, Mods.Select(x => x.Id));
    }

    /// <summary>
    /// Merge all mods :D
    /// </summary>
    /// <returns></returns>
    public async Task Merge()
    {
        Apply();

        string mergedOutput = Path.Combine(Config.Shared.StorageFolder, "merged");

        Directory.CreateDirectory(mergedOutput);

        await ToolHelper.Call("MalsMerger",
            "merge",
            string.Join('|', Mods.Select(x => Path.Combine(x.SourceFolder, "romfs"))), mergedOutput)
            .WaitForExitAsync();

        // Collect paths to the generated changelogs
        List<string> changelogPaths = new List<string>();
        foreach (var mod in Mods)
        {
            string rsdbFolderPath = Path.Combine(mod.SourceFolder, "romfs", "RSDB");

            // Generate changelog for each mod
            await ToolHelper.Call("RsdbMerge",
                "--generate-changelog", rsdbFolderPath,
                "--output", mod.SourceFolder)
                .WaitForExitAsync();

            // Check if the changelog file exists
            string changelogPath = Path.Combine(mod.SourceFolder, "rsdb.json");
            Console.WriteLine($"Checking for changelog at: {changelogPath}"); // Debugging check
            if (File.Exists(changelogPath))
            {
                changelogPaths.Add(changelogPath);
                Console.WriteLine($"Changelog found and added: {changelogPath}"); // Debugging check
            }
            else
            {
                Console.WriteLine($"Changelog not found at: {changelogPath}"); // Debugging check
            }
        }

        // Apply changelogs to merge RSDB file
        Directory.CreateDirectory(mergedOutput);

        if (changelogPaths.Any())
        {
            Console.WriteLine("Attempting to apply changelogs.");
            string changelogArguments = string.Join("|", changelogPaths);

            // Log the final command for debugging
            Console.WriteLine($"RsdbMerge command: --apply-changelogs {changelogArguments} --output {Path.Combine(mergedOutput, "romfs", "RSDB")} --version 121");

            await ToolHelper.Call("RsdbMerge",
                "--apply-changelogs", changelogArguments,
                "--output", Path.Combine(mergedOutput, "romfs", "RSDB"),
                "--version", "121")
                .WaitForExitAsync();
        }
        else
        {
            Console.WriteLine("No changelogs were found to apply.");
        }

        // After merging, execute Restbl on the merged mod folder
        await ToolHelper.Call("Restbl",
            "--action", "single-mod", // Ensure correct syntax for action argument
            "--use-checksums",
            "--version", "121",
            "--mod-path", mergedOutput,
            "--compress")
            .WaitForExitAsync();

        Console.WriteLine("Restbl tool execution completed."); // Debugging check
    }
}
