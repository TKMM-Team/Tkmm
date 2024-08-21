using FluentAvalonia.UI.Controls;
using Tkmm.Helpers;

namespace Tkmm.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PageAttribute : Attribute
{
    public PageAttribute(Page page, string description, Symbol icon, string? title = null,
        bool isDefault = false, bool isFooter = false)
    {
        Page = page;
        Title = title ?? Enum.GetName(page)!;
        Description = description;
        Icon = icon;
        IsDefault = isDefault;
        IsFooter = isFooter;
    }

    public string Title { get; init; }
    public string Description { get; init; }
    public Page Page { get; init; }
    public Symbol Icon { get; init; }
    public bool IsDefault { get; init; }
    public bool IsFooter { get; init; }
}

[AttributeUsage(AttributeTargets.Class)]
public class DontAutoRegisterAttribute : Attribute;