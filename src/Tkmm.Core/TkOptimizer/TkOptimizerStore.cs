using System.Text.Json;
using Tkmm.Core.Services;
using TkSharp.Core.Models;
using TkOptimizerConfigJson = System.Collections.Generic.Dictionary<string, Tkmm.Core.TkOptimizer.TkOptimizerProfile>;
using TkOptimizerConfig = System.Collections.Generic.Dictionary<System.Ulid, Tkmm.Core.TkOptimizer.TkOptimizerProfile>;

namespace Tkmm.Core.TkOptimizer;

public enum TkOptimizerStoreMode
{
    Immediate,
    Session
}

public class TkOptimizerStore(Ulid id, TkOptimizerStoreMode mode)
{
    private static readonly string _storeFilePath = Path.Combine(TKMM.ModManager.DataFolderPath, "tk-optimizer.json");
    private static readonly TkOptimizerConfig _store = FromDisk();

    public static TkOptimizerStore Current { get; set; } = Attach();
    
    public static void ResetCurrent(TkProfile? profile = null, TkOptimizerStoreMode mode = TkOptimizerStoreMode.Immediate)
    {
        Current = Attach(profile, mode);
    }

    public static TkOptimizerStore Attach(TkProfile? profile = null, TkOptimizerStoreMode mode = TkOptimizerStoreMode.Immediate)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();
        return new TkOptimizerStore(profile.Id, mode);
    }

    public static bool IsProfileEnabled(TkProfile? profile = null)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();
        return !_store.TryGetValue(profile.Id, out TkOptimizerProfile? optimizerProfile) || optimizerProfile.IsEnabled;
    }

    public bool IsEnabled {
        get => GetProfile().IsEnabled;
        set {
            GetProfile().IsEnabled = value;
            if (mode is TkOptimizerStoreMode.Immediate) Save();
        }
    }

    public string? Preset {
        get => GetProfile().Preset;
        set {
            GetProfile().Preset = value;
            if (mode is TkOptimizerStoreMode.Immediate) Save();
        }
    }

    public void Set<T>(string key, T value) where T : unmanaged
    {
        GetProfile().Values[key] = JsonSerializer.SerializeToElement(value);
        if (mode is TkOptimizerStoreMode.Immediate) Save();
    }

    public T Get<T>(string key, T @default) where T : unmanaged
    {
        return TryGet(key, out T result) ? result : @default;
    }

    public bool TryGet<T>(string key, out T value) where T : unmanaged
    {
        if (!GetProfile().Values.TryGetValue(key, out JsonElement json)) {
            value = default;
            return false;
        }

        value = json.Deserialize<T>();
        return true;
    }

    private TkOptimizerProfile GetProfile()
    {
        if (!_store.TryGetValue(id, out TkOptimizerProfile? profile)) {
            _store[id] = profile = new TkOptimizerProfile();
        }

        return profile;
    }

    private static TkOptimizerConfig FromDisk()
    {
        if (!File.Exists(_storeFilePath)) {
            return [];
        }

        using FileStream fs = File.OpenRead(_storeFilePath);
        return JsonSerializer.Deserialize<TkOptimizerConfigJson>(fs)?
            .ToDictionary(x => Ulid.Parse(x.Key), x => x.Value) ?? [];
    }

    private static void Save()
    {
        if (Path.GetDirectoryName(_storeFilePath) is string folder) {
            Directory.CreateDirectory(folder);
        }

        using FileStream fs = File.Create(_storeFilePath);
        JsonSerializer.Serialize(fs,
            _store.ToDictionary(x => x.Key.ToString(), x => x.Value)
        );
    }
}