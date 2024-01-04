using FluentAvalonia.UI.Controls;
using System.Collections.ObjectModel;
using Tcml.Helpers.Models;

namespace Tcml.Helpers;

public enum Page
{
    Home,
    Tools
}

public class PageManager
{
    private static readonly Lazy<PageManager> _shared = new(() => new());
    public static PageManager Shared => _shared.Value;

    public ObservableCollection<PageModel> Pages { get; } = [];

    public void Register(string title, object? content, Symbol icon, string? description = null)
    {
        Pages.Add(new PageModel {
            Title = title,
            Content = content,
            Description = description,
            Icon = icon
        });
    }
}
