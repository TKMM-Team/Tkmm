using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;

namespace Tkmm.Core.Models.Mods;

public partial class Mod : ObservableObject, IModItem
{
    private const string GB_MODS_URL = "https://gamebanana.com/mods/";

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
    private ObservableCollection<Guid> _optionGroupReferences = [];

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isEnabled = true;

    [ObservableProperty]
    [property: JsonIgnore]
    private string _sourceFolder = string.Empty;

    [JsonIgnore]
    public ObservableCollection<ModOptionGroup> OptionGroups { get; } = [];

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

    partial void OnSourceFolderChanged(string value)
    {
        OptionGroups.Clear();

        string optionsPath = Path.Combine(value, "options");
        if (Directory.Exists(optionsPath)) {
            foreach (var folder in Directory.EnumerateDirectories(optionsPath)) {
                OptionGroups.Add(ModOptionGroup.FromFolder(folder));
            }
        }
    }
}
