using System.Diagnostics.CodeAnalysis;

namespace Tkmm.Attributes;

// (c) NX-Editor, https://github.com/NX-Editor/NxEditor.PluginBase/blob/master/src/Attributes/MenuAttribute.cs

[AttributeUsage(AttributeTargets.Method)]
[method: SetsRequiredMembers]
public class MenuAttribute(string name, string path, string? hotkey = null, string? icon = null) : Attribute
{
    public required string Name { get; set; } = name;
    public required string Path { get; set; } = path;
    public string HotKey { get; set; } = hotkey ?? string.Empty;
    public string? Icon { get; set; } = icon;
    public bool IsSeparator { get; set; } = false;
    public string? GetCollectionMethodName { get; set; }
}