using Tkmm.Core.Components;
using Tkmm.Core;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Helpers;

public class ModHelper
{
    public static async Task<Mod?> Import(string arg)
    {
        try {
            AppStatus.Set($"Installing '{arg}'", "fa-solid fa-download", isWorkingStatus: true);

            Mod result = await Task.Run(async () => {
                return await ModManager.Shared.Import(arg);
            });

            AppStatus.Set("Install Complete!", "fa-regular fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
            return result;
        }
        catch (Exception ex) {
            AppLog.Log(ex);
            App.ToastError(ex);
            AppStatus.Set("Install Failed!", "fa-regular fa-circle-xmark", isWorkingStatus: false, temporaryStatusTime: 1.5);
        }

        return null;
    }
}
