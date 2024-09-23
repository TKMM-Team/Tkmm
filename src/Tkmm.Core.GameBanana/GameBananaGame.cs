using System.Text.Json.Serialization;

namespace Tkmm.Core.GameBanana;

internal class GameBananaGame
{
    [JsonPropertyName("_idRow")]
    public int Id { get; set; }
}