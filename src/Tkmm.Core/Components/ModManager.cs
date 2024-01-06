using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Text.Json;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

/// <summary>
/// This class will handle all of the
/// operations regarding loading the mod list
/// </summary>
public partial class ModManager : ObservableObject
{
    public string sarcToolPath { get; } = Path.Combine(Config.Shared.StaticStorageFolder, "TKMM.SarcTool.exe");

    public string rsdbToolPath { get; } = Path.Combine(Config.Shared.StaticStorageFolder, "rsdb-merge.exe");

    public string malsToolPath { get; } = Path.Combine(Config.Shared.StaticStorageFolder, "MalsMerger.exe");

    public string restblToolPath { get; } = Path.Combine(Config.Shared.StaticStorageFolder, "restbl.exe");

    // Singleton pattern - this means it's loaded once and never again

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
        // Good luck

        foreach (var mod in Mods) { }

        string sarcArgs = string.Empty;

        string rsdbArgs = string.Empty;

        string malsArgs = string.Empty; 

        string restblArgs = string.Empty;

    }
}
