using System.Text.Json.Serialization;

namespace Tkmm.Core.Models.GameBanana;

public class GameBananaGame
{
    [JsonPropertyName("_idRow")]
    public int Id { get; set; }
}
