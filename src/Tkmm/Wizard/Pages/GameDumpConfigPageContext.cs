using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Wizard.Pages;

public sealed partial class GameDumpConfigPageContext : ObservableObject
{
    [RelayCommand]
    private static async Task Browse(TextBox tb)
    {
        string? result = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            AllowMultiple = false
        }) switch {
            [IStorageFolder target] => target.TryGetLocalPath(),
            _ => null
        };

        tb.Text = result;
    }
}