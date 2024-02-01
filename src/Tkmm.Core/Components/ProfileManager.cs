using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Components.ModReaders;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

public partial class ProfileManager : ObservableObject
{
    private static readonly string _profilesMetadata = Path.Combine(Config.Shared.StorageFolder, "profiles.json");

    private static readonly Lazy<ProfileManager> _shared = new(() => new());
    public static ProfileManager Shared => _shared.Value;

    public static readonly string ModsFolder = Path.Combine(Config.Shared.StorageFolder, "mods");
    public static List<string> ExcludeFiles => [
        .. ToolHelper.ExcludeFiles,
        ".json",
        ".thumbnail"
    ];

    [ObservableProperty]
    private Profile _current;

    public ObservableCollection<Profile> Profiles { get; }
    public ObservableCollection<Mod> Mods { get; } = [];

    public ProfileManager()
    {
        foreach (var modFolder in Directory.EnumerateDirectories(ModsFolder)) {
            if (FolderModReader.FromInternal(modFolder) is Mod mod) {
                Mods.Add(mod);
            }
        }

        ProfileCollection? profileCollection = null;

        if (File.Exists(_profilesMetadata)) {
            using FileStream fs = File.OpenRead(_profilesMetadata);
            profileCollection = JsonSerializer.Deserialize<ProfileCollection>(fs);
        }

        Profiles = profileCollection?.Profiles ?? [new Profile("Default")];
        Current = Profiles[profileCollection?.CurrentIndex ?? 0];

        Profiles.CollectionChanged += (s, e) => {
            Apply();
        };
    }

    [RelayCommand]
    public void CreateNew()
    {
        Profile profile = new($"Profile {Profiles.Count + 1}");
        Profiles.Add(profile);
        Current = profile;
    }

    public static string GetModFolder(Mod mod)
    {
        return Path.Combine(ModsFolder, mod.Id.ToString());
    }

    public static string GetModFolder(Guid id)
    {
        return Path.Combine(ModsFolder, id.ToString());
    }

    public void Apply()
    {
        using FileStream fs = File.Create(_profilesMetadata);
        JsonSerializer.Serialize(fs, new ProfileCollection() {
            CurrentIndex = Profiles.IndexOf(Current),
            Profiles = Profiles
        });

        AppStatus.Set("Saved profiles!", "fa-solid fa-list-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
    }

    public async Task Merge()
    {
        Mod[] mods = Current.Mods
            .Where(x => x.IsEnabled && x.Mod is not null)
            .Select(x => x.Mod!)
            .Reverse()
            .ToArray();

        // If there are no mods, skip merging
        if (mods.Length <= 0) {
            AppStatus.Set("Nothing to Merge", "fa-solid fa-code-merge", isWorkingStatus: false, temporaryStatusTime: 1.5);
            AppLog.Log("No mods to merge!", LogLevel.Info);
            return;
        }

        AppStatus.Set($"Merging '{Current.Name}'", "fa-solid fa-code-merge");

        if (Directory.Exists(Config.Shared.MergeOutput)) {
            Directory.Delete(Config.Shared.MergeOutput, true);
        }

        Directory.CreateDirectory(Config.Shared.MergeOutput);

        Task[] tasks = [
            Task.Run(CopyContents),

            // Mals
            ToolHelper.Call(Tool.MalsMerger,
                "merge", string.Join('|', mods.Select(x => Path.Combine(x.SourceFolder, "romfs"))),
                Path.Combine(Config.Shared.MergeOutput, "romfs"),
                "--target", Config.Shared.GameLanguage
            ).WaitForExitAsync(),

            // Sarc
            ToolHelper.Call(Tool.SarcTool, [
                "merge",
                "--base", ModsFolder,
                "--mods", .. mods.Select(x => x.Id.ToString()),
                "--process", "All",
                "--output", Path.Combine(Config.Shared.MergeOutput, "romfs")
            ]).WaitForExitAsync(),

            // RSDB
            ToolHelper.Call(Tool.RsdbMerger,
                "--apply-changelogs", string.Join('|', mods.Select(x => x.SourceFolder)),
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
        AppLog.Log("Merge completed successfully", LogLevel.Info);
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
        foreach (var folder in TotkConfig.FileSystemFolders) {
            string srcFolder = Path.Combine(sourceFolder, folder);
            if (Directory.Exists(srcFolder)) {
                DirectoryOperations.CopyDirectory(srcFolder, Path.Combine(Config.Shared.MergeOutput, folder), ExcludeFiles, ToolHelper.ExcludeFolders, true);
            }
        }
    }
}
