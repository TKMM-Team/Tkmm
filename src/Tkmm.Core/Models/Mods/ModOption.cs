using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Generics;

namespace Tkmm.Core.Models.Mods;

[DebuggerDisplay("{Name}")]
public partial class ModOption : ObservableObject, IReferenceItem, IModItem
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private int? _priority;

    [ObservableProperty]
    private string? _thumbnailUri;

    [ObservableProperty]
    [property: JsonIgnore]
    private object? _thumbnail;

    [JsonIgnore]
    public string SourceFolder { get; private set; } = string.Empty;

    public static ModOption? FromFolder(string path)
    {
        ModOption option;

        if (TryGetMetadata(path, out string metadataPath)) {
            try {
                using FileStream fs = File.OpenRead(metadataPath);
                option = JsonSerializer.Deserialize<ModOption>(fs)!;
            }
            catch (Exception ex) {
                AppLog.Log(ex);
                return null;
            }
        }
        else {
            option = new() {
                Name = Path.GetFileName(path),
                Id = Guid.NewGuid()
            };
        }

        option.SourceFolder = path;
        return option;
    }

    private static bool TryGetMetadata(string path, out string metadataPath)
    {
        metadataPath = Path.Combine(path, "info.json");
        if (!File.Exists(metadataPath)) {
            return false;
        }

        return true;
    }

    async partial void OnThumbnailUriChanged(string? value)
    {
        if (Mod.ResolveThumbnail?.Invoke(this, false) is Task task) {
            await task;
        }
    }
}
