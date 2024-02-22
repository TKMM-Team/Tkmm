using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Components.Models;

public partial class Profile : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private ProfileMod? _selected;

    public ObservableCollection<ProfileMod> Mods { get; } = [];

    [JsonConstructor]
    public Profile(string name, ObservableCollection<ProfileMod> mods) : this(name)
    {
        Mods = mods;
        Mods.CollectionChanged += CollectionChanged;
    }

    public Profile(string name)
    {
        _name = name;
        Mods.CollectionChanged += CollectionChanged;
    }

    public ProfileMod Move(ProfileMod target, int offset)
    {
        int currentIndex = Mods.IndexOf(target);
        int newIndex = currentIndex + offset;

        if (newIndex < 0 || newIndex >= ProfileManager.Shared.Current.Mods.Count) {
            return target;
        }

        ProfileMod store = ProfileManager.Shared.Current.Mods[newIndex];
        ProfileManager.Shared.Current.Mods[newIndex] = target;
        ProfileManager.Shared.Current.Mods[currentIndex] = store;

        return target;
    }

    private void CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        ProfileManager.Shared.Apply();
    }

    partial void OnNameChanged(string value)
    {
        ProfileManager.Shared.Apply();
    }
}
