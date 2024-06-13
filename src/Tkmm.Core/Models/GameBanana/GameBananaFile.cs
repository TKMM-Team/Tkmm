using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Models.GameBanana;

public partial class GameBananaFile : ObservableObject
{
    [JsonPropertyName("_sFile")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("_sDescription")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("_sDownloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("_sMd5Checksum")]
    public string Checksum { get; set; } = string.Empty;

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isSelected = false;
}
