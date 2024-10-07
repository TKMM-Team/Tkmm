using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Tkmm.Models;

namespace Tkmm.Components;

public enum Page
{
    Home,
    Profiles,
    Tools,
    ShopParam,
    Mods,
    About,
    Logs,
    Settings,
}

public partial class PageManager : ObservableObject
{
    private static readonly Lazy<PageManager> _shared = new(() => new());
    public static PageManager Shared => _shared.Value;

    [ObservableProperty]
    private PageModel? _current;

    [ObservableProperty]
    private Page _default;

    private readonly Dictionary<Page, (int Index, bool IsFooter)> _lookup = [];
    public ObservableCollection<PageModel> Pages { get; } = [];
    public ObservableCollection<PageModel> FooterPages { get; } = [];

    public PageModel this[Page page] {
        get {
            (int index, bool isFooter) = _lookup[page];
            return (isFooter ? FooterPages : Pages)[index];
        }
    }

    public void Register(Page page, string title, object? content, Symbol icon, string? description = null, bool isDefault = false, bool isFooter = false)
    {
        ObservableCollection<PageModel> source = isFooter ? FooterPages : Pages;
        _lookup[page] = (source.Count, isFooter);

        source.Add(new PageModel {
            Title = title,
            Content = content,
            Description = description,
            Icon = icon
        });

        if (isDefault) {
            Default = page;
        }
    }

    public void Focus(Page page)
    {
        Current = this[page];
    }

    public T Get<T>(Page page) where T : ObservableObject
    {
        (int index, bool isFooter) = _lookup[page];
        if ((isFooter ? FooterPages : Pages)[index].Content is UserControl { DataContext: T value }) {
            return value;
        }

        throw new InvalidOperationException(
            $"Invalid ViewModel type for '{page}'");
    }
}
