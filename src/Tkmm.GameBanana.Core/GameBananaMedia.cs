using System.Text.Json.Serialization;

namespace Tkmm.GameBanana.Core;

public class GameBananaMedia
{
    [JsonPropertyName("_aImages")]
    public List<GameBananaImage> Images { get; set; } = [];
}
