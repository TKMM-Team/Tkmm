using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Wizard.Pages;

public sealed partial class KeysFolderPageContext : ObservableObject
{
    [ObservableProperty]
    private string? _keysFolderPath;

    [RelayCommand]
    private static async Task Browse(TextBox tb)
    {
        var result = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = Locale["SetupWizard_SelectKeysFolder"],
            AllowMultiple = false
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (result is not null) {
            tb.Text = result;
        }
    }
} 