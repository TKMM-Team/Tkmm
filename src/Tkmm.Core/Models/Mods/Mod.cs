using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;

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
    private IModImporter? _importer;

    public static Mod FromFile(string file)
    {
        FileStream fs = File.OpenRead(file);
        ZipArchive archive = new(fs, mode: ZipArchiveMode.Read, leaveOpen: true);

        if (archive.Entries.FirstOrDefault(x => x.Name == "info.json")?.Open() is Stream stream) {
            if (JsonSerializer.Deserialize<Mod>(stream) is Mod mod) {
                if (archive.Entries.FirstOrDefault(x => x.Name == mod.ThumbnailUri)?.Open() is Stream thumbnailStream) {
                    string tmpThumbnailPath = Path.GetTempFileName();
                    using FileStream tmpThumbnail = File.Create(tmpThumbnailPath);
                    thumbnailStream.CopyTo(tmpThumbnail);
                    mod.ThumbnailUri = tmpThumbnailPath;
                }

                mod._importer = new ArchiveModImporter(archive);
                return mod;
            }
        }

        throw new InvalidOperationException("""
            Error parsing tkcl file
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

        result._importer = new FolderModImporter(folder);
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
        if (IsFromStorage || _importer is null) {
            return;
        }

        _importer.Import(SourceFolder = Path.Combine(Config.Shared.StorageFolder, "mods", Id.ToString()));
        _importer = null;
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
