using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.GameBanana;
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
    private string _version = string.Empty;

    [ObservableProperty]
    private string _author = string.Empty;

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

    public static async Task<Mod> FromUri(string uri)
    {
        if (File.Exists(uri)) {
            return FromFile(uri);
        }
        else if (Directory.Exists(uri)) {
            return FromFolder(uri);
        }

        string id;
        if (uri.StartsWith(GB_MODS_URL)) {
            id = Path.GetRelativePath(GB_MODS_URL, uri)
                .Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
        }
        else if (int.TryParse(uri, out _)) {
            id = uri;
        }
        else {
            throw new ArgumentException($"""
                Invalid path, url, or mod id: '{uri}'
                """, nameof(uri));
        }

        return await GameBananaMod.FromId(id);
    }

    public static Mod FromFile(string file)
    {
        using FileStream fs = File.OpenRead(file);
        return FromStream(fs, file);
    }

    public static Mod FromStream(Stream input, string file)
    {
        if (ModReaderProviderService.GetReader(file) is IModReader parser) {
            return parser.Parse(input, file);
        }

        throw new NotSupportedException($"""
            The provided file '{file}' is not a supported file type.
            """);
    }

    public static Mod FromFolder(string folder)
    {
        string modInfoPath = Path.Combine(folder, "info.json");
        if (!File.Exists(modInfoPath)) {
            throw new NotSupportedException(
                "Please use the packager to prep your mod for use in TKMM!");
        }

        using FileStream fs = File.OpenRead(modInfoPath);
        Mod result = JsonSerializer.Deserialize<Mod>(fs)
            ?? throw new InvalidOperationException(
                "Could not parse ModInfo");

        return result;
    }

    public Mod()
    {
        OptionGroups.CollectionChanged += (_, e)
            => ReferenceCollectionHelper.ResolveCollectionChanged(OptionGroupReferences, e);

        // TODO: remove this before releasing xD
        SourceFolder = "D:\\bin\\mods\\master-mode";
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
