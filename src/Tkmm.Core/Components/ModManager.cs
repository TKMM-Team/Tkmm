﻿using CommunityToolkit.Mvvm.ComponentModel;
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

        string output = Path.Combine(Config.Shared.StorageFolder, "merged");
        Directory.CreateDirectory(output);

        await ToolHelper.Call("MalsMerger",
            "merge",
            string.Join('|', Mods.Select(x => Path.Combine(x.SourceFolder, "romfs"))), output)
            .WaitForExitAsync();
    }
}
