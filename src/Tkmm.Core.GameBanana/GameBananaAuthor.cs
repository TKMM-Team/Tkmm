using System.Text.Json.Serialization;

namespace Tkmm.Core.GameBanana;

internal class GameBananaAuthor
{
    [JsonPropertyName("_sRole")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("_sName")]
    public string Name { get; set; } = string.Empty;   
}