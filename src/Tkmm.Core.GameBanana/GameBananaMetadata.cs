using System.Text.Json.Serialization;

namespace Tkmm.Core.GameBanana;

internal class GameBananaMetadata
{
    [JsonPropertyName("_bIsComplete")]
    public bool IsCompleted { get; set; }
}
