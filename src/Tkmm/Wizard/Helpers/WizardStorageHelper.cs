using Avalonia.Platform.Storage;
using Tkmm.Wizard.Models;

namespace Tkmm.Wizard.Helpers;

public static class WizardStorageHelper
{
    private static async Task<string?> PickFolderAsync(string? title, bool allowMultiple = false)
        => await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = title,
            AllowMultiple = allowMultiple
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

    public static async Task<string?> PickFileAsync(string title, string name, params string[] patterns)
        => await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType(name) { Patterns = patterns }]
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

    public static Task<string?> BrowseAsync(WizardBrowseOptions options)
        => PickFolderAsync(options.Title, options.AllowMultiple);

    public static async ValueTask<bool> ApplyFolder(string title, Action<string> apply)
    {
        if (await PickFolderAsync(title) is not { } path) {
            return false;
        }

        apply(path);
        return true;
    }
}