using System.Text.Json.Serialization;

namespace Tkmm.Core.Models.GameBanana;

public class GameBananaCreditGroup
{
    [JsonPropertyName("_sGroupName")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("_aAuthors")]
    public List<GameBananaAuthor> Authors { get; set; } = [];
}
