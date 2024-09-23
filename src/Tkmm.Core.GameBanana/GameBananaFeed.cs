using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Tkmm.Core.GameBanana;

internal class GameBananaFeed
{
    [JsonPropertyName("_aMetadata")]
    public GameBananaMetadata Metadata { get; set; } = new();

    [JsonPropertyName("_aRecords")]
    public ObservableCollection<GameBananaModInfo> Records { get; set; } = [];
}

[JsonSerializable(typeof(GameBananaFeed))]
internal partial class GameBananaFeedJsonContext : JsonSerializerContext;
