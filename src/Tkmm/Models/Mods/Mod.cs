using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core;

namespace Tkmm.Models.Mods;

public partial class Mod : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.Empty;

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
    private bool _isEnabled = true;
    
    [ObservableProperty]
    [property: JsonIgnore]
    private Bitmap? _thumbnail;

    [JsonIgnore]
    public string SourceFolder { get; private set; } = string.Empty;

    [JsonIgnore]
    public bool IsFromStorage { get; private set; } = false;

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

        // Resolve thumbnail
        if (result.ThumbnailUri is string uri) {
            string localPath = Path.Combine(folder, uri);
            if (File.Exists(localPath)) {
                result.Thumbnail = new(localPath);
            }

            //
            // URL image support (broken)

            // else if (uri.StartsWith("https://")) {
            //     try {
            //         using HttpClient client = new();
            //         using Stream stream = await client.GetStreamAsync(uri);
            //         mod.Thumbnail = new(stream);
            //     }
            //     catch (Exception ex) {
            //         Trace.WriteLine($"""
            //             Error reading thumbnail URL: '{uri}'

            //             Exception:
            //             {ex}
            //             """);
            //     }
            // }
        }

        return result;
    }

    public void Import()
    {
        if (IsFromStorage) {
            return;
        }

        string outputModFolder = Path.Combine(Config.Shared.StorageFolder, "mods", Id.ToString());
        DirectoryOperations.CopyDirectory(SourceFolder, outputModFolder);
    }
}
