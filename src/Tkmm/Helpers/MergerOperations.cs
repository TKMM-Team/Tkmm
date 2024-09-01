using Avalonia.Controls.Notifications;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Services;

namespace Tkmm.Helpers;

public static class MergerOperations
{
    public static async Task Merge()
    {
        App.LogTkmmInfo();

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
