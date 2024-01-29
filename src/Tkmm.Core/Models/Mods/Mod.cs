using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Services;

namespace Tkmm.Core.Models.Mods;

public partial class Mod : ObservableObject, IModItem
{
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

    [JsonIgnore]
    public bool IsFromStorage { get; private set; } = false;

    [JsonIgnore]
    internal IModImporter? Importer { get; set; }

    public static Mod FromFile(string file)
    {
        using FileStream fs = File.OpenRead(file);
        return FromFile(fs, file);
    }

    public static Mod FromFile(Stream input, string file)
    {
        if (ModParserService.GetParser(file) is IModParser parser) {
            return parser.Parse(input, file);
        }

        throw new NotSupportedException($"""
            The provided file '{file}' is not a supported file type.
            """);
    }

    public static Mod FromFolder(string folder, bool isFromStorage = false)
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

        result.Importer = new FolderModImporter(folder);
        result.SourceFolder = folder;
        result.IsFromStorage = isFromStorage;

        return result;
    }

    public Mod()
    {
        OptionGroups.CollectionChanged += (_, e)
            => ReferenceCollectionHelper.ResolveCollectionChanged(OptionGroupReferences, e);

        // TODO: remove this before releasing xD
        SourceFolder = "D:\\bin\\mods\\master-mode";
    }

    public void Import()
    {
        if (IsFromStorage || Importer is null) {
            return;
        }

        Importer.Import(SourceFolder = Path.Combine(Config.Shared.StorageFolder, "mods", Id.ToString()));
        Importer = null;
        IsFromStorage = true;
    }

    public void StageImport(string path)
    {
        IsFromStorage = false;
        SourceFolder = path;
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
