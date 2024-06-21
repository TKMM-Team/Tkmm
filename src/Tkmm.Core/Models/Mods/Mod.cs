using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Tkmm.Core.Components;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;

namespace Tkmm.Core.Models.Mods;

public partial class Mod : ObservableObject, IModItem, IReferenceItem
{
    public static Func<IModItem, bool, Task>? ResolveThumbnail { get; set; }

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
    [property: JsonIgnore]
    private object? _thumbnail;

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isEditingOptions;

    [ObservableProperty]
    private ObservableCollection<Guid> _optionGroupReferences = [];

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<ModOptionGroup> _optionGroups = [];

    [JsonIgnore]
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

    public static async Task<Mod> FromStream(Stream stream, string path, Guid? modId)
    {
        if (ModReaderProviderService.GetReader(path) is IModReader parser) {
            return await parser.Read(stream, path, modId);
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
    [property: JsonIgnore]
    public void AddToCurrentProfile()
    {
        if (!ProfileManager.Shared.Current.Mods.Contains(this)) {
            ProfileManager.Shared.Current.Mods.Add(this);
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
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
    [property: JsonIgnore]
    public void RefreshOptions()
    {
        RefreshOptions(SourceFolder);
    }

    [RelayCommand]
    [property: JsonIgnore]
    public void NewContributor()
    {
        Contributors.Add(new());
    }

    [RelayCommand]
    [property: JsonIgnore]
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

    /// <summary>
    /// Recursively selects the <see cref="Mod"/> and it's selected mod options.
    /// </summary>
    public IEnumerable<IModItem> SelectModRecursive()
    {
        return [this, ..OptionGroups
            .OrderBy(x => x.Priority)
            .SelectMany(x => x.SelectedOptions.OrderBy(x => x.Priority))
            .Reverse()
            .Cast<IModItem>()
        ];
    }

    async partial void OnThumbnailUriChanged(string? value)
    {
        if (ResolveThumbnail?.Invoke(this, true) is Task task) {
            await task;
        }
    }

    partial void OnIsEditingOptionsChanged(bool value)
    {
        if (value) {
            ProfileManager.Shared.Current.Selected = this;
        }
    }
}
