using System.Text.Json.Serialization;

namespace Tkmm.Core.Models.GameBanana;

public class GameBananaMod
{
    [JsonPropertyName("_bIsFlagged")]
    public bool IsFlagged { get; set; }

    [JsonPropertyName("_bIsTrashed")]
    public bool IsTrashed { get; set; }

    [JsonPropertyName("_aFiles")]
    public List<GameBananaFile> Files { get; set; } = [];

    [JsonPropertyName("_sText")]
    public string Text { get; set; } = string.Empty;
}
