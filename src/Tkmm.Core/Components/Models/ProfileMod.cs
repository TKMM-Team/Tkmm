using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Tkmm.Core.Generics;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Core.Components.Models;

public partial class ProfileMod : ObservableObject, IReferenceItem
{
    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private Guid _id;

    [JsonIgnore]
    public Mod? Mod => ProfileManager.Shared.Mods.FirstOrDefault(x => x.Id == Id);

    public static implicit operator ProfileMod(Mod mod) => new(mod);
    public ProfileMod(Mod mod)
    {
        Id = mod.Id;
        PropertyChanged += AppWhenPropertyChanged;
    }

    [JsonConstructor]
    public ProfileMod(Guid id, bool isEnabled)
    {
        Id = id;
        IsEnabled = isEnabled;
        PropertyChanged += AppWhenPropertyChanged;
    }

    public override bool Equals(object? obj)
    {
        if (obj is ProfileMod profileMod) {
            return Id.Equals(profileMod.Id);
        }

        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    private static void AppWhenPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsEnabled)) {
            ProfileManager.Shared.Apply();
        }
    }
}
