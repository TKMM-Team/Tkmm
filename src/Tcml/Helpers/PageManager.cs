using FluentAvalonia.UI.Controls;
using Tcml.Helpers.Models;

namespace Tcml.Helpers;

public enum Page
{
    Home,
    Tools,
    About
}

public class PageManager
{
    private static readonly Lazy<PageManager> _shared = new(() => new());
    public static PageManager Shared => _shared.Value;

    public Dictionary<Page, PageModel> Pages { get; } = [];

    public void Register(Page page, string title, object? content, Symbol icon, string? description = null)
    {
        Pages.Add(page, new PageModel {
            Title = title,
            Content = content,
            Description = description,
            Icon = icon
        });
    }
}
