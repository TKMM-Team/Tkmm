using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Components.ModReaders;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components;

public partial class ProfileManager : ObservableObject
{
    private static readonly string _profilesMetadata = Path.Combine(Config.Shared.StorageFolder, "profiles.json");

    private static readonly Lazy<ProfileManager> _shared = new(() => new());
    public static ProfileManager Shared => _shared.Value;

    public static readonly string ModsFolder = Path.Combine(Config.Shared.StorageFolder, "mods");

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

        AppLog.Log("Profiles updated", LogLevel.Info);
    }

    partial void OnCurrentChanged(Profile value)
    {
        Apply();
    }
}
