using System.Text.Json.Serialization;

namespace Tkmm.Core.GameBanana;

public class GameBananaSubmitter
{
    [JsonPropertyName("_sName")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("_sProfileUrl")]
    public string Url { get; set; } = string.Empty;
}
