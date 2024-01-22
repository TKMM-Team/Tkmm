using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Models.GameBanana;

public class GameBananaFeed
{
    [JsonPropertyName("_aMetadata")]
    public GameBananaMetadata Metadata { get; set; } = new();

    [JsonPropertyName("_aRecords")]
    public ObservableCollection<GameBananaModInfo> Records { get; set; } = [];
}
