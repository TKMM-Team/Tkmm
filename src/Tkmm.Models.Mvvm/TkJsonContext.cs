using System.Text.Json.Serialization;

namespace Tkmm.Models.Mvvm;

[JsonSerializable(typeof(TkMod))]
[JsonSerializable(typeof(TkModOption))]
[JsonSerializable(typeof(TkModOptionGroup))]
public sealed partial class TkJsonContext : JsonSerializerContext;