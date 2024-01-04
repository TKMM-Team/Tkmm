using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using System.Collections.ObjectModel;
using Tkmm.Helpers.Models;

namespace Tkmm.Helpers;

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

    private readonly Dictionary<Page, (int Index, bool IsFooter)> _lookup = [];
    public ObservableCollection<PageModel> Pages { get; } = [];
    public ObservableCollection<PageModel> FooterPages { get; } = [];

    public void Register(Page page, string title, object? content, Symbol icon, string? description = null, bool isFooter = false)
    {
        ObservableCollection<PageModel> source = isFooter ? FooterPages : Pages;
        _lookup[page] = (source.Count, isFooter);

        source.Add(new PageModel {
            Title = title,
            Content = content,
            Description = description,
            Icon = icon
        });
    }

    public T Get<T>(Page page) where T : ObservableObject
    {
        (int index, bool isFooter) = _lookup[page];
        if ((isFooter ? FooterPages : Pages)[index].Content is UserControl userControl) {
            if (userControl.DataContext is T value) {
                return value;
            }
        }

        throw new InvalidOperationException(
            $"Invalid ViewModel type for '{page}'");
    }
}
