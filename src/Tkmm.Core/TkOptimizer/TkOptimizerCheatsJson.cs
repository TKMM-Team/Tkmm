using System.Text.Json.Serialization;

namespace Tkmm.Core.TkOptimizer;

public class TkOptimizerCheatsJson : List<TkOptimizerCheatsJson.Cheat>
{
    public sealed class Cheat : Dictionary<string, string>
    {
        public string DisplayVersion => this["Aversion"];
        
        public string Version => this[nameof(Version)];
        
        public IEnumerable<(string, string)> Cheats => this
            .Select(kv => (kv.Key, kv.Value))
            .Where(x => x.Key is not ("Aversion" or "Version" or "Source" or "Cheat Example")
                        && x.Value.Length > 1 && x.Value[0] == '[');
    }
}

[JsonSerializable(typeof(TkOptimizerCheatsJson))]
public partial class TkOptimizerCheatsJsonContext : JsonSerializerContext;