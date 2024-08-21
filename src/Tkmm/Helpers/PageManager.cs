using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using SharpCompress;
using Tkmm.Attributes;
using Tkmm.Helpers.Models;

namespace Tkmm.Helpers;

public enum Page
{
    Home = 0,
    Profiles = 1,
    Tools = 2,
    ShopParam = 3,
    Mods = 4,
    
    Logs = 5,
    Settings = 6
}

public partial class PageManager : ObservableObject
{
    private static readonly Lazy<PageManager> _shared = new(() => new());
    public static PageManager Shared => _shared.Value;

    [ObservableProperty]
    private PageModel? _current = null;

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
            Focus(page);
        }
    }

    public void Focus(Page page)
    {
        Current = this[page];
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Init(
        Action<PageManager>? beforeNormals = null,
        Action<PageManager>? afterNormals = null,
        Action<PageManager>? beforeFooters = null,
        Action<PageManager>? afterFooters = null)
    {
        var pageMetas = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x =>
                x.GetCustomAttribute<PageAttribute>() is not null &&
                x.GetCustomAttribute<DontAutoRegisterAttribute>() is null
            )
            .Select(x => (type: x, attribute: x.GetCustomAttribute<PageAttribute>()!))
            .ToArray();

        var (normals, footers) = (
            pageMetas.Where(x => !x.attribute.IsFooter).OrderBy(x => x.attribute.Page),
            pageMetas.Where(x => x.attribute.IsFooter).OrderBy(x => x.attribute.Page)
        );

        beforeNormals?.Invoke(Shared);
        normals.ForEach(page => Shared.Register(
            page: page.attribute.Page,
            title: page.attribute.Title,
            content: page.type.GetConstructor(Type.EmptyTypes)?.Invoke([])!,
            icon: page.attribute.Icon,
            description: page.attribute.Description,
            isDefault: page.attribute.IsDefault,
            isFooter: page.attribute.IsFooter
        ));
        afterNormals?.Invoke(Shared);

        beforeFooters?.Invoke(Shared);
        footers.ForEach(page => Shared.Register(
            page: page.attribute.Page,
            title: page.attribute.Title,
            content: page.type.GetConstructor(Type.EmptyTypes)?.Invoke([])!,
            icon: page.attribute.Icon,
            description: page.attribute.Description,
            isDefault: page.attribute.IsDefault,
            isFooter: page.attribute.IsFooter
        ));
        afterFooters?.Invoke(Shared);
    }
}
