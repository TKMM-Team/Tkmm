#if !SWITCH

using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Tkmm.Controls;
using Tkmm.Core;
using Tkmm.Core.Models;
using Tkmm.Dialogs;

namespace Tkmm.Helpers;

public static class ExportLocationsHelper
{
    /// <summary>
    /// Prompts the user to create export locations if none are currently configured.
    /// </summary>
    /// <returns><see langword="false"/> if the calling operation should be cancelled</returns>
    public static async ValueTask<bool> CreateExportLocations()
    {
        if (!string.IsNullOrWhiteSpace(Config.Shared.MergeOutput) || Config.Shared.ExportLocations.Any(x => x.IsEnabled)) {
            return true;
        }

        MessageDialogResult result = await MessageDialog.Show(
            TkLocale.System_Popup_CreateExportLocation,
            TkLocale.System_Popup_CreateExportLocation_Title,
            MessageDialogButtons.YesNoCancel,
            MessageDialogs.CreateExportLocationsDialog);

        if (result is not MessageDialogResult.Yes) {
            return result is not MessageDialogResult.Cancel;
        }
        
        bool isFirstSelection = true;

    RequestAnother:
        FolderPickerOpenOptions options = new() {
            Title = Locale[TkLocale.Tip_BrowseFolder],
            AllowMultiple = false
        };

        IReadOnlyList<IStorageFolder> browseResult = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(options);

        if (browseResult is not [IStorageFolder target] || target.TryGetLocalPath() is not string path) {
            goto RequestNext;
        }

        if (isFirstSelection) {
            Config.Shared.MergeOutput = path;
            goto RequestNext;
        }
        
        Config.Shared.ExportLocations.Add(new ExportLocation {
            SymlinkPath = path,
            IsEnabled = true
        });

    RequestNext:
        bool addAnother = await MessageDialog.Show(
            TkLocale.System_Popup_AddExportLocation,
            TkLocale.System_Popup_CreateExportLocation_Title,
            MessageDialogButtons.YesNo) is MessageDialogResult.Yes;

        if (addAnother) {
            isFirstSelection = false;
            goto RequestAnother;
        }

        await Config.Shared.ExportLocations.Create();
        return true;
    }
    
    public static async ValueTask OpenEditorDialog(ExportLocations source)
    {
        TaskDialog dialog = new() {
            DataContext = source,
            Buttons = [
                TaskDialogButton.CloseButton
            ],
            Title = Locale[TkLocale.EditExportLocations_Title],
            Content = new ExportLocationCollectionEditor(),
            XamlRoot = App.XamlRoot
        };

        await dialog.ShowAsync();
        await Config.Shared.ExportLocations.Create();
    }
}
#endif