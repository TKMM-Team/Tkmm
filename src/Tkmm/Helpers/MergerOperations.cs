using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using Tkmm.Builders;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Services;

namespace Tkmm.Helpers;

public static class MergerOperations
{
    public static async Task Merge()
    {
        App.LogTkmmInfo();

        if (!Config.Shared.ExportLocations.Any(x => x.IsEnabled)) {
            ContentDialog dialog = new() {
                Title = "Warning",
                Content = "There are currently no export locations enabled. Would you like to configure them now?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            if (await dialog.ShowAsync() is ContentDialogResult.Primary) {
                await ExportLocationControlBuilder.Edit(Config.Shared.ExportLocations);
            }
        }

        try {
            await MergerService.Merge();
            App.Toast(
                $"The profile '{ProfileManager.Shared.Current.Name}' was merged successfully.",
                "Merge Successful!",
                NotificationType.Success,
                TimeSpan.FromDays(5)
            );
        }
        catch (Exception ex) {
            TriviaService.Stop();
            App.ToastError(ex);
            AppLog.Log(ex);
            AppStatus.Set(ex.Message, isWorkingStatus: false, temporaryStatusTime: 1.5, logLevel: LogLevel.None);
        }
    }
}
