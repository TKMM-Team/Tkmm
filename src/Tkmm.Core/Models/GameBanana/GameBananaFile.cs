using System.Text.Json.Serialization;

namespace Tkmm.Core.Models.GameBanana;

public class GameBananaFile
{
    [JsonPropertyName("_sFile")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("_sDownloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("_sMd5Checksum")]
    public string Checksum { get; set; } = string.Empty;
}
