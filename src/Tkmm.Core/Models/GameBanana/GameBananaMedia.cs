using System.Text.Json.Serialization;

namespace Tkmm.Core.Models.GameBanana;

public class GameBananaMedia
{
    [JsonPropertyName("_aImages")]
    public List<GameBananaImage> Images { get; set; } = [];
}
