using Avalonia.Platform.Storage;
using Tkmm.Core;
using Tkmm.Core.Models;
using Tkmm.Dialogs;

namespace Tkmm.Helpers;

public static class ExportLocationsHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>false if the calling operation should be cancelled</returns>
    public static async ValueTask<bool> CreateExportLocations()
    {
        if (Config.Shared.ExportLocations.Any(x => x.IsEnabled)) {
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

    RequestAnother:
        FolderPickerOpenOptions options = new() {
            Title = Locale[TkLocale.Tip_BrowseFolder],
            AllowMultiple = false
        };

        IReadOnlyList<IStorageFolder> browseResult = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(options);

        if (browseResult is not [IStorageFolder target] || target.TryGetLocalPath() is not string path) {
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
            goto RequestAnother;
        }

        return true;
    }
}