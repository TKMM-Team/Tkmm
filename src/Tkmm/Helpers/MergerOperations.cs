using Tkmm.Core;
using Tkmm.Core.Services;

namespace Tkmm.Helpers;

public static class MergerOperations
{
    public static async Task Merge()
    {
        try {
            await MergerService.Merge();
        }
        catch (Exception ex) {
            TriviaService.Stop();
            App.ToastError(ex);
            AppLog.Log(ex);
            AppStatus.Set(ex.Message, isWorkingStatus: false, temporaryStatusTime: 1.5, logLevel: LogLevel.None);;
        }
    }
}
