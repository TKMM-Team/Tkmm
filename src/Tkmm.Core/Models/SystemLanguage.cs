using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Models;

[JsonConverter(typeof(SystemLanguageJsonConverter))]
public readonly record struct SystemLanguage(string Value)
{
    public string DisplayName => GetCultureNameFromLocale(Value);
    
    public static implicit operator SystemLanguage(string language) => new(language);
}

public sealed class SystemLanguageJsonConverter : JsonConverter<SystemLanguage>
{
    public override SystemLanguage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()!;
    }

    public override void Write(Utf8JsonWriter writer, SystemLanguage value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}