using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Models;

[JsonConverter(typeof(GameLanguageJsonConverter))]
public readonly record struct GameLanguage(string Value, string DisplayName)
{
    public static implicit operator string(GameLanguage language) => language.Value;
}

public sealed class GameLanguageJsonConverter : JsonConverter<GameLanguage>
{
    public override GameLanguage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        string value = reader.GetString()!;
        reader.Read();
        string displayName = reader.GetString()!;
        reader.Read();
        
        return new GameLanguage(value, displayName);
    }

    public override void Write(Utf8JsonWriter writer, GameLanguage value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Value);
        writer.WriteStringValue(value.DisplayName);
        writer.WriteEndArray();
    }
}