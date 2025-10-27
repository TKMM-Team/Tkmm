using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Attributes;
using Tkmm.Core.Models;
using Tkmm.Helpers;

namespace Tkmm.Controls;

public partial class PathCollectionEditor : TemplatedControl
{
    public static readonly StyledProperty<PathCollection> ValueProperty = AvaloniaProperty.Register<PathCollectionEditor, PathCollection>(nameof(Value));

    public static readonly StyledProperty<PathType> TypeProperty = AvaloniaProperty.Register<PathCollectionEditor, PathType>(nameof(Type));

    public static readonly StyledProperty<string?> FileBrowserFilterProperty = AvaloniaProperty.Register<PathCollectionEditor, string?>(nameof(FileBrowserFilter));

    public static readonly StyledProperty<string?> FileBrowserSuggestedFileNameProperty = AvaloniaProperty.Register<PathCollectionEditor, string?>(nameof(FileBrowserSuggestedFileName));

    public static readonly StyledProperty<string?> BrowserTitleProperty = AvaloniaProperty.Register<PathCollectionEditor, string?>(nameof(BrowserTitle));

    public PathCollection Value {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public PathType Type {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public string? FileBrowserFilter {
        get => GetValue(FileBrowserFilterProperty);
        set => SetValue(FileBrowserFilterProperty, value);
    }

    public string? FileBrowserSuggestedFileName {
        get => GetValue(FileBrowserSuggestedFileNameProperty);
        set => SetValue(FileBrowserSuggestedFileNameProperty, value);
    }

    public string? BrowserTitle {
        get => GetValue(BrowserTitleProperty);
        set => SetValue(BrowserTitleProperty, value);
    }

    public PathCollectionEditor() : this(PathType.FileOrFolder)
    {
    }

    public PathCollectionEditor(PathType type)
    {
        Type = type;
    }

    [RelayCommand]
    private async Task OpenFile(PathCollectionItem item)
    {
        FilePickerOpenOptions options = new() {
            Title = Locale[TkLocale.Tip_BrowseFile],
            AllowMultiple = false
        };

        options.Title = BrowserTitle;
        options.SuggestedFileName = FileBrowserSuggestedFileName;
        options.FileTypeFilter = FileFilterHelper.ParseFilterString(FileBrowserFilter);

        var result = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(options);

        if (result is not [{ } target] || target.TryGetLocalPath() is not { } path) {
            return;
        }

        item.Target = path;
    }

    [RelayCommand]
    private async Task OpenFolder(PathCollectionItem item)
    {
        FolderPickerOpenOptions options = new() {
            Title = Locale[TkLocale.Tip_BrowseFolder],
            AllowMultiple = false
        };

        options.Title = BrowserTitle;
        options.SuggestedFileName = FileBrowserSuggestedFileName;

        var result = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(options);

        if (result is not [{ } target] || target.TryGetLocalPath() is not { } path) {
            return;
        }

        item.Target = path;
    }

    [RelayCommand]
    private void Delete(PathCollectionItem item)
    {
        Value.Remove(item);
    }
}