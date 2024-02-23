using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tkmm.Core.Components;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;

namespace Tkmm.Core.Models.Mods;

public partial class Mod : ObservableObject, IModItem
{
    public static Func<Mod, Task>? ResolveThumbnail { get; set; }

    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _version = "1.0.0";

    [ObservableProperty]
    private string _author = Config.Shared.DefaultAuthor;

    [ObservableProperty]
    private ObservableCollection<ModContributor> _contributors = [];

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string? _thumbnailUri;

    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private object? _thumbnail;

    [ObservableProperty]
    private ObservableCollection<Guid> _optionGroupReferences = [];

    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private ObservableCollection<ModOptionGroup> _optionGroups = [];

    public string SourceFolder => ProfileManager.GetModFolder(Id);

    public static async Task<Mod> FromPath(string path)
    {
        if (ModReaderProviderService.GetReader(path) is IModReader parser) {
            if (File.Exists(path)) {
                using FileStream fs = File.OpenRead(path);
                return await parser.Read(fs, path);
            }
            else {
                return await parser.Read(null, path);
            }
        }

        throw new NotSupportedException($"""
            The provided input '{path}' is not supported.
            """);
    }

    public static async Task<Mod> FromStream(Stream stream, string path)
    {
        if (ModReaderProviderService.GetReader(path) is IModReader parser) {
            return await parser.Read(stream, path);
        }

        throw new NotSupportedException($"""
            The provided file '{path}' is not supported.
            """);
    }

    public Mod()
    {
        OptionGroups.CollectionChanged += (_, e)
            => ReferenceCollectionHelper.ResolveCollectionChanged(OptionGroupReferences, e);
    }

    [RelayCommand]
    [property: System.Text.Json.Serialization.JsonIgnore]
    public void AddToCurrentProfile()
    {
        if (!ProfileManager.Shared.Current.Mods.Contains(this)) {
            ProfileManager.Shared.Current.Mods.Add(this);
        }
    }

    [RelayCommand]
    [property: System.Text.Json.Serialization.JsonIgnore]
    public void Uninstall()
    {
        ProfileManager.Shared.Mods.Remove(this);
        foreach (var profile in ProfileManager.Shared.Profiles) {
            profile.Mods.Remove(this);
        }

        // Only delete the mod folder if
        // it is in the system directory
        if (Path.GetDirectoryName(SourceFolder) is string folder && folder == ProfileManager.ModsFolder) {
            Directory.Delete(SourceFolder, true);
        }

        ProfileManager.Shared.Apply();
    }

    [RelayCommand]
    [property: System.Text.Json.Serialization.JsonIgnore]
    public void RefreshOptions()
    {
        RefreshOptions(SourceFolder);
    }

    [RelayCommand]
    [property: System.Text.Json.Serialization.JsonIgnore]
    public void NewContributor()
    {
        Contributors.Add(new());
    }

    [RelayCommand]
    [property: System.Text.Json.Serialization.JsonIgnore]
    public void RemoveContributor(ModContributor contributor)
    {
        Contributors.Remove(contributor);
    }

    public void RefreshOptions(string path)
    {
        OptionGroups.Clear();

        string optionsPath = Path.Combine(path, "options");
        if (Directory.Exists(optionsPath)) {
            foreach (var folder in Directory.EnumerateDirectories(optionsPath)) {
                OptionGroups.Add(ModOptionGroup.FromFolder(folder));
            }
        }

        OptionGroups = [.. OptionGroups.OrderBy(x => x.Name)];
    }

    async partial void OnThumbnailUriChanged(string? value)
    {
        if (ResolveThumbnail?.Invoke(this) is Task task) {
            await task;
        }
    }
}
