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
            App.ToastError(ex);
            AppLog.Log(ex);
        }
    }
}
