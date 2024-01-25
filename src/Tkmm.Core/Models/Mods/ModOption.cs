using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using Tkmm.Core.Generics;

namespace Tkmm.Core.Models.Mods;

public partial class ModOption : ObservableObject, IReferenceItem
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    public static ModOption FromFolder(string path)
    {
        ModOption option;

        if (TryGetMetadata(path, out string metadataPath)) {
            using FileStream fs = File.OpenRead(metadataPath);
            option = JsonSerializer.Deserialize<ModOption>(fs)!;
        }
        else {
            option = new() {
                Name = Path.GetFileName(path),
                Id = Guid.NewGuid()
            };
        }

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
}
