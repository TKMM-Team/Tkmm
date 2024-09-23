using System.Text.Json.Serialization;

namespace Tkmm.Core.GameBanana;

public class GameBananaMetadata
{
    [JsonPropertyName("_bIsComplete")]
    public bool IsCompleted { get; set; }
}
