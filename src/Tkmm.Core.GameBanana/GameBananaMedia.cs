using System.Text.Json.Serialization;

namespace Tkmm.Core.GameBanana;

internal class GameBananaMedia
{
    [JsonPropertyName("_aImages")]
    public List<GameBananaImage> Images { get; set; } = [];
}
