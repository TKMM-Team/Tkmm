#if !SWITCH
using FluentAvalonia.UI.Controls;
using Tkmm.Controls;
using Tkmm.Core;
using Tkmm.Core.Models;

namespace Tkmm.Helpers;

public static class ExportLocationsHelper
{
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
        Config.Shared.ExportLocations.Create();
    }
}
#endif