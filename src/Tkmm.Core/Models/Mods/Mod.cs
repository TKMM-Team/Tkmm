using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Models.Mods;

public partial class Mod : ObservableObject
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
    private bool _isEnabled = true;

    [ObservableProperty]
    [property: JsonIgnore]
    private string _sourceFolder = string.Empty;

    [JsonIgnore]
    public bool IsFromStorage { get; private set; } = false;

    public static Mod FromFile(string file)
    {
        throw new NotImplementedException();
    }

    public static Mod FromFolder(string folder, bool isFromStorage = false)
    {
        string modInfoPath = Path.Combine(folder, "info.json");
        if (!File.Exists(modInfoPath)) {
            throw new NotSupportedException(
                "Mods with no info.json is not supported");
        }

        using FileStream fs = File.OpenRead(modInfoPath);
        Mod result = JsonSerializer.Deserialize<Mod>(fs)
            ?? throw new InvalidOperationException(
                "Could not parse ModInfo");

        result.SourceFolder = folder;
        result.IsFromStorage = isFromStorage;

        return result;
    }

    public void Import()
    {
        if (IsFromStorage) {
            return;
        }

        string outputModFolder = Path.Combine(Config.Shared.StorageFolder, "mods", Id.ToString());
        DirectoryOperations.CopyDirectory(SourceFolder, outputModFolder, overwrite: true);
        SourceFolder = outputModFolder;
        IsFromStorage = true;
    }

    public void StageImport(string path)
    {
        IsFromStorage = false;
        SourceFolder = path;
    }
}
