using System.Text.Json.Serialization;

namespace Tkmm.GameBanana.Core;

public class GameBananaMetadata
{
    [JsonPropertyName("_bIsComplete")]
    public bool IsCompleted { get; set; }
}
