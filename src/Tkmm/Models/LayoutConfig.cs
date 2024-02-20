using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core;

namespace Tkmm.Models;

public partial class LayoutConfig : ObservableObject
{
    [JsonIgnore]
    public string Name { get; private set; } = "Generic";

    public GridUnitType TopPanelGridUnitType { get; set; } = GridUnitType.Star;
    public double TopPanelValue { get; set; } = 1;

    public GridUnitType LowerPanelGridUnitType { get; set; } = GridUnitType.Star;
    public double LowerPanelValue { get; set; } = 1.8;

    [ObservableProperty]
    [property: JsonIgnore]
    private GridLength _topPanel = GridLength.Star;

    [ObservableProperty]
    [property: JsonIgnore]
    private GridLength _lowerPanel = new(0.7, GridUnitType.Star);

    public static LayoutConfig Load(string name)
    {
        string file = GetPath(name);
        if (!File.Exists(file)) {
            return new() {
                Name = name
            };
        }

        using FileStream fs = File.OpenRead(file);
        LayoutConfig result = JsonSerializer.Deserialize<LayoutConfig>(fs) ?? new();

        result.Name = name;
        result.TopPanel = new(result.TopPanelValue, result.TopPanelGridUnitType);
        result.LowerPanel = new(result.LowerPanelValue, result.LowerPanelGridUnitType);

        return result;
    }

    public void Save()
    {
        using FileStream fs = File.Create(GetPath(Name));
        JsonSerializer.Serialize(fs, this);
    }

    private static string GetPath(string name)
    {
        string folder = Path.Combine(Config.Shared.StaticStorageFolder, "Layout");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, $"{name}.json");
    }

    partial void OnTopPanelChanged(GridLength value)
    {
        TopPanelGridUnitType = value.GridUnitType;
        TopPanelValue = value.Value;
    }

    partial void OnLowerPanelChanged(GridLength value)
    {
        LowerPanelGridUnitType = value.GridUnitType;
        LowerPanelValue = value.Value;
    }
}
