using System.Runtime.InteropServices;
using System.Text.Json;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer.Models;
using TkSharp.Core.Models;

namespace Tkmm.Core.TkOptimizer;

public class TkOptimizerStore(Ulid id)
{
    private static readonly string StoreFilePath =
        Path.Combine(TKMM.ModManager.DataFolderPath, "tk-optimizer.json");

    private static readonly Dictionary<Ulid, TkOptimizerProfile> Store = FromDisk();

    public static TkOptimizerStore Current => CreateStore(TKMM.ModManager.GetCurrentProfile());

    public static TkOptimizerStore CreateStore(TkProfile? profile = null)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();
        return new TkOptimizerStore(profile.Id);
    }

    public static void Remove(TkProfile profile)
    {
        Store.Remove(profile.Id);
        Save();
    }

    public static bool IsProfileEnabled(TkProfile? profile = null)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();
        return !Store.TryGetValue(profile.Id, out var optimizerProfile) || optimizerProfile.IsEnabled;
    }

    public bool IsEnabled {
        get => GetProfile().IsEnabled;
        set {
            GetProfile().IsEnabled = value;
            Save();
            TKMM.MergeBasic();
            TkOptimizerService.Context.ApplyToSdCard();
        }
    }

    public string? Preset {
        get => GetProfile().Preset;
        set {
            GetProfile().Preset = value;
            Save();
            TKMM.MergeBasic();
        }
    }

    public void SetCheat(TkOptimizerCheatGroup cheat, string key, bool isEnabled)
    {
        var cheatProfileGroup = GetCheatGroup(cheat.Version);
        switch (isEnabled) {
            case true:
                cheatProfileGroup.Add(key);
                break;
            case false:
                cheatProfileGroup.Remove(key);
                break;
        }

        Save();
        TKMM.MergeBasic();
    }

    public bool GetCheat(TkOptimizerCheatGroup cheat, string key)
    {
        return GetCheatGroup(cheat.Version).Contains(key);
    }

    private TkOptimizerProfile GetProfile()
    {
        ref var profile = ref CollectionsMarshal.GetValueRefOrAddDefault(Store, id, out var exists);
        if (!exists || profile is null) {
            profile = new TkOptimizerProfile();
        }

        return profile;
    }

    private HashSet<string> GetCheatGroup(string version)
    {
        var profile = GetProfile();

        ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault(profile.Cheats, version, out var exists);
        if (!exists || group is null) {
            group = [];
        }

        return group;
    }

    private static Dictionary<Ulid, TkOptimizerProfile> FromDisk()
    {
        try {
            if (!File.Exists(StoreFilePath) || new FileInfo(StoreFilePath) is { Length: 0 }) {
                return [];
            }

            using var fs = File.OpenRead(StoreFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, TkOptimizerProfile>>(fs)?
                .ToDictionary(x => Ulid.Parse(x.Key), x => x.Value) ?? [];
        }
        catch {
            return [];
        }
    }

    private static void Save()
    {
        if (Path.GetDirectoryName(StoreFilePath) is { } folder) {
            Directory.CreateDirectory(folder);
        }

        using var fs = File.Create(StoreFilePath);
        JsonSerializer.Serialize(fs,
            Store.ToDictionary(static x => x.Key.ToString(), static x => x.Value));
    }
}
