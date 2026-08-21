using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Models;

[JsonConverter(typeof(TkmmModeJsonConverter))]
public readonly record struct TkmmMode(string Value)
{
    public string DisplayName => Value switch {
        "Switch" => Locale["TkmmMode_Switch"],
        "Hybrid" => Locale["TkmmMode_Hybrid"],
        _ => Locale["TkmmMode_Emulator"]
    };

    public static implicit operator string(TkmmMode mode) => mode.Value;

    public bool IsEmulator => Value is "Emulator";
    public bool IsSwitch => Value is "Switch";
    public bool IsHybrid => Value is "Hybrid";
}

public sealed class TkmmModeJsonConverter : JsonConverter<TkmmMode>
{
    public override TkmmMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? "Emulator");

    public override void Write(Utf8JsonWriter writer, TkmmMode value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
