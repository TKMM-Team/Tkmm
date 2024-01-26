using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;

namespace Tkmm.Core.Models.Mods;

public enum ModOptionGroupType
{
    Multi,
    MultiRequired,
    Single,
    SingleRequired
}

public partial class ModOptionGroup : ObservableObject, IReferenceItem, IModItem
{
    public static readonly ModOptionGroupType[] OptionGroupTypes = Enum.GetValues<ModOptionGroupType>();

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ModOptionGroupType _type;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _thumbnailUri = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ModOptionDependency> _dependencies = [];

    [ObservableProperty]
    private ObservableCollection<Guid> _optionReferences = [];

    [JsonIgnore]
    public ObservableCollection<ModOption> Options { get; } = [];

    [JsonIgnore]
    public string SourceFolder { get; private set; } = string.Empty;

    public ModOptionGroup()
    {
        Options.CollectionChanged += (_, e)
            => ReferenceCollectionHelper.ResolveCollectionChanged(OptionReferences, e);
    }

    public static ModOptionGroup FromFolder(string path)
    {
        ModOptionGroup group;

        if (TryGetMetadata(path, out string metadataPath)) {
            using FileStream fs = File.OpenRead(metadataPath);
            group = JsonSerializer.Deserialize<ModOptionGroup>(fs)!;
        }
        else {
            group = new() {
                Name = Path.GetFileName(path),
                Id = Guid.NewGuid()
            };
        }

        foreach (var folder in Directory.EnumerateDirectories(path)) {
            group.Options.Add(ModOption.FromFolder(folder));
        }

        group.SourceFolder = path;
        return group;
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
