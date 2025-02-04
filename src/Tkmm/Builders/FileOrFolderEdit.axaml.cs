using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Models;

namespace Tkmm.Builders;

public partial class FileOrFolderEdit : TemplatedControl
{
    public static readonly StyledProperty<FileOrFolder> ValueProperty = AvaloniaProperty.Register<FileOrFolderEdit, FileOrFolder>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<string?> FileBrowserFilterProperty = AvaloniaProperty.Register<FileOrFolderEdit, string?>(nameof(FileBrowserFilter));
    
    public static readonly StyledProperty<string?> FileBrowserSuggestedFileNameProperty = AvaloniaProperty.Register<FileOrFolderEdit, string?>(nameof(FileBrowserSuggestedFileName));
    
    public static readonly StyledProperty<string?> BrowserTitleProperty = AvaloniaProperty.Register<FileOrFolderEdit, string?>(nameof(BrowserTitle));

    public FileOrFolder Value {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
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
    
    [RelayCommand]
    private async Task OpenFile()
    {
        FilePickerOpenOptions options = new() {
            Title = Locale[TkLocale.Tip_BrowseFile],
            AllowMultiple = false
        };

        options.Title = BrowserTitle;
        options.SuggestedFileName = FileBrowserSuggestedFileName;
        options.FileTypeFilter = ParseFilterString(FileBrowserFilter);
        
        IReadOnlyList<IStorageFile> result = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(options);
        
        if (result is not [IStorageFile target] || target.TryGetLocalPath() is not string path) {
            return;
        }

        Value = path;
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        FolderPickerOpenOptions options = new() {
            Title = Locale[TkLocale.Tip_BrowseFolder],
            AllowMultiple = false
        };
        
        options.Title = BrowserTitle;
        options.SuggestedFileName = FileBrowserSuggestedFileName;
        
        IReadOnlyList<IStorageFolder> result = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(options);

        if (result is not [IStorageFolder target] || target.TryGetLocalPath() is not string path) {
            return;
        }

        Value = path;
    }
    
    private static FilePickerFileType[] ParseFilterString(string? filter = null)
    {
        if (filter != null) {
            try {
                string[] groups = filter.Split('|');
                var types = new FilePickerFileType[groups.Length];

                for (int i = 0; i < groups.Length; i++) {
                    string[] pair = groups[i].Split(':');
                    types[i] = new FilePickerFileType(pair[0]) {
                        Patterns = pair[1].Split(';')
                    };
                }

                return types;
            }
            catch {
                throw new FormatException(
                    $"Could not parse filter arguments '{filter}'.\n" +
                    $"Example: \"Yaml Files:*.yml;*.yaml|All Files:*.*\"."
                );
            }
        }

        return [];
    }
}