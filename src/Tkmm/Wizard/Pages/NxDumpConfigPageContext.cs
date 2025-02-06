using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core;

namespace Tkmm.Wizard.Pages
{
    public sealed partial class NxDumpConfigPageContext : ObservableObject
    {
        private static readonly FilePickerFileType _nsp = new("NSP")
        {
            Patterns = new[] { "*.nsp" }
        };

        private static readonly FilePickerFileType _xci = new("XCI")
        {
            Patterns = new[] { "*.xci" }
        };

        private static readonly FilePickerFileType _xciNsp = new("XCI/NSP")
        {
            Patterns = new[] { "*.xci", "*.nsp" }
        };

        public NxDumpConfigPageContext()
        {
        }

        [RelayCommand]
        private static async Task Browse(TextBox target)
        {
            string? result = target.Tag switch
            {
                "folder" => await BrowseFolder(),
                _ => await BrowseFile(target.Tag as string)
            };

            target.Text = result;
        }

        private static async ValueTask<string?> BrowseFolder()
        {
            return await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false
            }) switch
            {
                [IStorageFolder target] => target.TryGetLocalPath(),
                _ => null
            };
        }

        private static async ValueTask<string?> BrowseFile(string? types)
        {
            return await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = types switch
                {
                    "xci,nsp" => new[] { _xciNsp, _xci, _nsp },
                    _ => new[] { _nsp }
                }
            }) switch
            {
                [IStorageFile target] => target.TryGetLocalPath(),
                _ => null
            };
        }
    }
} 