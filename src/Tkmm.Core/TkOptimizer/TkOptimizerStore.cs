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
    private static readonly Dictionary<Ulid, bool> Enabled = LoadEnabled();
    private static readonly Dictionary<Ulid, TkOptimizerProfile> SessionProfiles = [];

    public static TkOptimizerStore Current => CreateStore(TKMM.ModManager.GetCurrentProfile());

    public static TkOptimizerStore CreateStore(TkProfile? profile = null)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();
        return new TkOptimizerStore(profile.Id);
    }

    public static void Remove(TkProfile profile)
    {
        Enabled.Remove(profile.Id);
        SessionProfiles.Remove(profile.Id);
        SaveEnabled();
    }

    public static bool IsProfileEnabled(TkProfile? profile = null)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();
        return !Enabled.TryGetValue(profile.Id, out var on) || on;
    }

    public bool IsEnabled {
        get => !Enabled.TryGetValue(id, out var on) || on;
        set {
            Enabled[id] = value;
            SaveEnabled();
            TKMM.MergeBasic();
            TkOptimizerService.Context.ApplyToSdCard();
        }
    }

    public string? Preset {
        get => GetSessionProfile().Preset;
        set {
            GetSessionProfile().Preset = value;
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

        TKMM.MergeBasic();
    }

    public bool GetCheat(TkOptimizerCheatGroup cheat, string key)
    {
        return GetCheatGroup(cheat.Version).Contains(key);
    }

    private TkOptimizerProfile GetSessionProfile()
    {
        ref var profile = ref CollectionsMarshal.GetValueRefOrAddDefault(SessionProfiles, id, out var exists);
        if (!exists || profile is null) profile = new TkOptimizerProfile();

        return profile;
    }

    private HashSet<string> GetCheatGroup(string version)
    {
        var profile = GetSessionProfile();

        ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault(profile.Cheats, version, out var exists);
        if (!exists || group is null) group = [];

        return group;
    }

    private static Dictionary<Ulid, bool> LoadEnabled()
    {
        try {
            if (!File.Exists(StoreFilePath) || new FileInfo(StoreFilePath) is { Length: 0 }) {
                return [];
            }

            var raw = JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(StoreFilePath));
            if (raw is null) {
                return [];
            }

            Dictionary<Ulid, bool> map = [];
            foreach (var (key, value) in raw) {
                if (Ulid.TryParse(key, out var profileId)) {
                    map[profileId] = value;
                }
            }

            return map;
        }
        catch {
            return [];
        }
    }

    private static void SaveEnabled()
    {
        if (Path.GetDirectoryName(StoreFilePath) is { } folder) {
            Directory.CreateDirectory(folder);
        }

        var serial = Enabled.ToDictionary(static x => x.Key.ToString(), static x => x.Value);
        File.WriteAllText(StoreFilePath, JsonSerializer.Serialize(serial));
    }
}
