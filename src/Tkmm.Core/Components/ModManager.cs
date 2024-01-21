using CommunityToolkit.Mvvm.ComponentModel;
using Octokit;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.Mods;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices; // Include this for ZipFile

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
    /// Import a mod from a .tkcl file or folder
    /// </summary>
    /// <returns></returns>
    public Mod Import(string path)
    {
        // Check if the file is a ZIP file
        if (File.Exists(path) && Path.GetExtension(path).Equals(".tkcl", StringComparison.OrdinalIgnoreCase))
        {   
            // Extract the ZIP file to a new directory
            string extractPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));

            if (Path.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            ZipFile.ExtractToDirectory(path, extractPath);

            // Use the extracted folder path for further processing
            path = extractPath;
        }

        Mod mod = File.Exists(path)
            ? Mod.FromFile(path) : Mod.FromFolder(path);

        // Check for existing mods
        if (Mods.FirstOrDefault(x => x.Id == mod.Id) is Mod existing)
        {
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
        // Check if the "merged" folder exists and delete it if it does
        if (Directory.Exists(mergedOutput))
        {
            Directory.Delete(mergedOutput, true);
        }
        Directory.CreateDirectory(mergedOutput);

        // Define the file extensions and subfolders to exclude
        var excludedExtensions = new HashSet<string> { ".rsizetable.zs", ".byml.zs", ".bgyml", ".pack.zs", ".sarc.zs", ".blarc.zs"};

        await ToolHelper.Call("MalsMerger",
            "merge",
            string.Join('|', Mods.Select(x => Path.Combine(x.SourceFolder, "romfs"))), Path.Combine(mergedOutput, "romfs"))
            .WaitForExitAsync();

        // Collect paths to the mods' source folders
        List<string> modPaths = new List<string>();
        List<string> modNames = new List<string>();
        foreach (var mod in Mods)
        {
            foreach (var file in Directory.EnumerateFiles(mod.SourceFolder, "*.*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(file);

                // Compute the relative directory path

                // Skip the file if its name ends with any of the excluded extensions
                if (excludedExtensions.Any(ex => fileInfo.Name.EndsWith(ex)))
                    continue;

                // Compute the destination path
                var relativePath = fileInfo.FullName.Substring(mod.SourceFolder.Length + 1);
                var destinationPath = Path.Combine(mergedOutput, relativePath);

                // Create the destination directory if it doesn't exist
                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);

                // Copy the file
                File.Copy(file, destinationPath, true);
            }

            string exefsPath = Path.Combine(mod.SourceFolder, "exefs");
            string destinationDir = Path.Combine(mergedOutput, "exefs");

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
            // Add mod's source folder to the list (assuming the source folder is the required path)
            modPaths.Add(mod.SourceFolder); // Enclosing in quotes
            modNames.Add(mod.Id.ToString()); // Enclosing in quotes
        }

        string ConvertPaths(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var paths = input.Split('|');
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = paths[i].Trim();
            }

            return string.Join(" ", paths);
        }

        // Apply changelogs to merge RSDB files
        Directory.CreateDirectory(mergedOutput);

        string modPathsArguments = string.Join("|", modPaths);

        string modPathsArguments2 = string.Join("|", modNames);

        string basePath = Path.Combine(Config.Shared.StorageFolder, "mods");

        string convertedPath = ConvertPaths(modPathsArguments2);

        // Run the first SARC merger command.
        await ToolHelper.Call("SarcTool",
            "merge",
            "--base", basePath,
            "--mods", convertedPath,
            "--process", "All",
            "--output", Path.Combine(mergedOutput, "romfs"))
            .WaitForExitAsync();

        if (modPaths.Any())
        {
            Console.WriteLine("Attempting to apply changelogs.");

            string rsdbFolder = Path.Combine(mergedOutput, "romfs", "RSDB");
            Directory.CreateDirectory(rsdbFolder);

            Console.WriteLine(modPathsArguments);

            await ToolHelper.Call("RsdbMerge",
                "--apply-changelogs", modPathsArguments, "--output", rsdbFolder, "--version", TotkConfig.Shared.Version.ToString())
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
            "--version", TotkConfig.Shared.Version.ToString(),
            "--mod-path", mergedOutput,
            "--compress")
            .WaitForExitAsync();

        Console.WriteLine("Restbl tool execution completed."); // Debugging check

        Console.WriteLine("Merging Complete!");
    }
}
