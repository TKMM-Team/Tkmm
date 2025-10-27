using System.Runtime.InteropServices;
using System.Text.Json;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer.Models;
using TkSharp.Core.Models;
using TkOptimizerConfigJson = System.Collections.Generic.Dictionary<string, Tkmm.Core.TkOptimizer.TkOptimizerProfile>;
using TkOptimizerConfig = System.Collections.Generic.Dictionary<System.Ulid, Tkmm.Core.TkOptimizer.TkOptimizerProfile>;

namespace Tkmm.Core.TkOptimizer;

public class TkOptimizerStore(Ulid id)
{
    private static readonly string _storeFilePath = Path.Combine(TKMM.ModManager.DataFolderPath, "tk-optimizer.json");
    private static readonly TkOptimizerConfig _store = FromDisk();

    public static TkOptimizerStore Current => CreateStore(TKMM.ModManager.GetCurrentProfile());

    public static TkOptimizerStore CreateStore(TkProfile? profile = null)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();
        return new TkOptimizerStore(profile.Id);
    }

    public static bool Remove(TkProfile profile)
    {
        return _store.Remove(profile.Id);
    }

    public static bool IsProfileEnabled(TkProfile? profile = null)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();
        return !_store.TryGetValue(profile.Id, out var optimizerProfile) || optimizerProfile.IsEnabled;
    }

    public bool IsEnabled {
        get => GetProfile().IsEnabled;
        set {
            GetProfile().IsEnabled = value;
            Save();
            TKMM.MergeBasic();
        }
    }

    public string? Preset {
        get => GetProfile().Preset;
        set {
            GetProfile().Preset = value;
            Save();
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

    public void Set<T>(string key, T value) where T : unmanaged
    {
        GetProfile().Values[key] = JsonSerializer.SerializeToElement(value);
        Save();
    }

    public T Get<T>(string key, T @default) where T : unmanaged
    {
        return TryGet(key, out T result) ? result : @default;
    }

    public bool TryGet<T>(string key, out T value) where T : unmanaged
    {
        if (!GetProfile().Values.TryGetValue(key, out var json)) {
            value = default;
            return false;
        }

        value = json.Deserialize<T>();
        return true;
    }

    private TkOptimizerProfile GetProfile()
    {   
        ref var profile = ref CollectionsMarshal.GetValueRefOrAddDefault(_store, id, out var exists);
        if (!exists || profile is null) profile = new TkOptimizerProfile();

        return profile;
    }

    private HashSet<string> GetCheatGroup(string version)
    {
        var profile = GetProfile();
        
        ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault(profile.Cheats, version, out var exists);
        if (!exists || group is null) group = [];

        return group;
    }

    private static TkOptimizerConfig FromDisk()
    {
        if (!File.Exists(_storeFilePath) || new FileInfo(_storeFilePath) is { Length: 0 }) {
            return [];
        }

        using var fs = File.OpenRead(_storeFilePath);
        return JsonSerializer.Deserialize<TkOptimizerConfigJson>(fs)?
            .ToDictionary(x => Ulid.Parse(x.Key), x => x.Value) ?? [];
    }

    private static void Save()
    {
        if (Path.GetDirectoryName(_storeFilePath) is { } folder) {
            Directory.CreateDirectory(folder);
        }

        using var fs = File.Create(_storeFilePath);
        JsonSerializer.Serialize(fs,
            _store.ToDictionary(x => x.Key.ToString(), x => x.Value)
        );
        
        TkOptimizerService.Context.ApplyToMergedOutput();
    }
}