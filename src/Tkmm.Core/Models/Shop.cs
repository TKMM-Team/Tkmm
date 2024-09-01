using System.Text.Json.Serialization;

namespace Tkmm.Core.Models
{
    public class Shop
    {
        [JsonPropertyName("Shop Name")]
        public string ShopName { get; set; } = string.Empty;

        [JsonPropertyName("NPC Name")]
        public string NpcName { get; set; } = string.Empty;

        [JsonPropertyName("Location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("Required Quest")]
        public string RequiredQuest { get; set; } = string.Empty;

        [JsonPropertyName("NPC ActorName")]
        public string NpcActorName { get; set; } = string.Empty;

        [JsonPropertyName("Map")]
        public string Map { get; set; } = "Surface";

        [JsonPropertyName("Coordinates")]
        public Coordinates Coordinates { get; set; } = new();

        public override string ToString()
        {
            return $"""
                {ShopName}
                {NpcName} in {Location} after completeing {RequiredQuest}
                [{Coordinates.X}, {Coordinates.Y}, {Map}]
                [{NpcActorName}]
                """;
        }
    }
}
