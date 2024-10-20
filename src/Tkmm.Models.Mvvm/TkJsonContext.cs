using System.Text.Json.Serialization;

namespace Tkmm.Models.Mvvm;

[JsonSerializable(typeof(TkMod))]
[JsonSerializable(typeof(TkModOption))]
[JsonSerializable(typeof(TkModOptionGroup))]
[JsonSerializable(typeof(TkProfile))]
[JsonSerializable(typeof(TkModStorage))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public sealed partial class TkJsonContext : JsonSerializerContext;