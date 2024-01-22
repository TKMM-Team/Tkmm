using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tkmm.Core.Models
{
    public class Shop
    {
        [JsonPropertyName("Shop Name")]
        public string ShopName { get; set; }

        [JsonPropertyName("NPC Name")]
        public string NPCName { get; set; }
        
        public string Location { get; set; }
        
        [JsonPropertyName("Required Quest")]
        public string RequiredQuest { get; set; }
        
        [JsonPropertyName("NPC ActorName")]
        public string NPCActorName { get; set; }
    }
}
