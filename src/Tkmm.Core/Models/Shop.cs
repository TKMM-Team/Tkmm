using System.Text.Json.Serialization;

namespace Tkmm.Core.Models
{
    public class Shop
    {
        [JsonPropertyName("Shop Name")]
        public string ShopName { get; set; } = string.Empty;

        [JsonPropertyName("NPC Name")]
        public string NPCName { get; set; } = string.Empty;
        
        [JsonPropertyName("Location")]
        public string Location { get; set; } = string.Empty;
        
        [JsonPropertyName("Required Quest")]
        public string RequiredQuest { get; set; } = string.Empty;
        
        [JsonPropertyName("NPC ActorName")]
        public string NPCActorName { get; set; } = string.Empty;

        [JsonPropertyName("Map")]
        public string Map { get; set; } = "Surface";

        [JsonPropertyName("Coordinates")]
        public Coordinates Coordinates { get; set; } = new();
    }
}
