using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Dialogs;

namespace Tkmm.Actions;

public sealed partial class ImportActions : ActionsBase<ImportActions>
{
    public static readonly FilePickerFileType SupportedFormats = new("Supported Formats") {
        Patterns = ["*.tkcl","*.zip","*.rar","*.7z"]
    };
    
    public static readonly FilePickerFileType TkclFormat = new("TKCL") {
        Patterns = ["*.tkcl"]
    };

    protected override string ActionGroupName { get; } = nameof(ImportActions).Humanize();

    [RelayCommand]
    public async ValueTask ImportFromFile(CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        IReadOnlyList<IStorageFile> results = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = "Import from File",
            AllowMultiple = true,
            FileTypeFilter = [
                SupportedFormats,
                TkclFormat,
                FilePickerFileTypes.All
            ]
        });

        if (results.Count <= 0) {
            return;
        }

        foreach (IStorageFile targetFile in results) {
            try {
                await using Stream stream = await targetFile.OpenReadAsync();
                await TKMM.ModManager.Import(targetFile.Name, stream, ct);
            }
            catch (Exception ex) {
                TKMM.Logger.LogError(ex, "An error occured while importing the file '{TargetFile}'.", targetFile?.Name);
                await ErrorDialog.ShowAsync(ex);
            }
        }
    }

    [RelayCommand]
    public async ValueTask ImportFromFolder(CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        IReadOnlyList<IStorageFolder> results = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = "Import from Folder",
            AllowMultiple = true
        });

        if (results.Count <= 0) {
            return;
        }

        foreach (IStorageFolder targetFolder in results) {
            try {
                if (targetFolder.TryGetLocalPath() is not string folder) {
                    continue;
                }
                
                await TKMM.ModManager.Import(folder, stream: null, ct);
            }
            catch (Exception ex) {
                TKMM.Logger.LogError(ex, "An error occured while importing the folder '{TargetFolder}'.", targetFolder?.Name);
                await ErrorDialog.ShowAsync(ex);
            }
        }
        
    }

    [RelayCommand]
    public async ValueTask ImportFromArgument(CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }
        
        ContentDialog dialog = new() {
            Title = "Import from Argument",
            Content = new TextBox {
                Watermark = "Argument (File, Folder, URL, Mod ID)"
            },
            PrimaryButtonText = "Import",
            SecondaryButtonText = "Cancel",
        };
        
        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return;
        }
        
        if (dialog.Content is TextBox { Text: not null } textBox) {
            string argument = textBox.Text;
            try {
                await TKMM.ModManager.Import(argument, stream: null, ct);
            }
            catch (Exception ex) {
                TKMM.Logger.LogError(ex, "An error occured while importing the argument '{Argument}'.", argument);
                await ErrorDialog.ShowAsync(ex);
            }
        }
    }
}