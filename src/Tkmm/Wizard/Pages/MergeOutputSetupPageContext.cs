using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Wizard.Pages;

public sealed partial class MergeOutputSetupPageContext : ObservableObject
{
    [ObservableProperty]
    private string _mergeOutputPath = string.Empty;

    [RelayCommand]
    private static async Task Browse(TextBox tb)
    {
        var result = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
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