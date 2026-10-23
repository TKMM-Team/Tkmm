using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Models;

[JsonConverter(typeof(SwitchFirmwareVersionJsonConverter))]
public readonly record struct SwitchFirmwareVersion(string Value)
{
    public bool IsFirmware20OrHigher => Value is "Firmware20OrHigher";
    
    public string DisplayName => IsFirmware20OrHigher
        ? Locale["Config_Firmware_20OrHigher"]
        : Locale["Config_Firmware_19OrLower"];

    public static implicit operator string(SwitchFirmwareVersion firmware) => firmware.Value;
}

public sealed class SwitchFirmwareVersionJsonConverter : JsonConverter<SwitchFirmwareVersion>
{
    public override SwitchFirmwareVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? "Firmware19OrLower");

    public override void Write(Utf8JsonWriter writer, SwitchFirmwareVersion value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}