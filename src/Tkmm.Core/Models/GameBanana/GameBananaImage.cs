using System.Text.Json.Serialization;

namespace Tkmm.Core.Models.GameBanana;

public class GameBananaImage
{
    [JsonPropertyName("_sBaseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("_sFile530")]
    public string File { get; set; } = string.Empty;
}
