using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using System.Collections.ObjectModel;
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

    private readonly Dictionary<Page, int> _lookup = [];
    public ObservableCollection<PageModel> Pages { get; } = [];

    public void Register(Page page, string title, object? content, Symbol icon, string? description = null)
    {
        _lookup[page] = Pages.Count;

        Pages.Add(new PageModel {
            Title = title,
            Content = content,
            Description = description,
            Icon = icon
        });
    }

    public T Get<T>(Page page) where T : ObservableObject
    {
        if (Pages[_lookup[page]].Content is UserControl userControl) {
            if (userControl.DataContext is T value) {
                return value;
            }
        }

        throw new InvalidOperationException(
            $"Invalid ViewModel type for '{page}'");
    }
}
