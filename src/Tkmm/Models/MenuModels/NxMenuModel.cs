#if SWITCH

using System.Diagnostics;
using MenuFactory.Abstractions.Attributes;
using static Tkmm.Core.Localization.StringResources_Menu;

namespace Tkmm.Models.MenuModels;

public class NxMenuModel
{
    [Menu(NX_REBOOT, NX_MENU, Icon = "fa-solid fa-rotate")]
    public static void Reboot()
    {
        ExecuteCommand("/usr/bin/tkmm-reboot.sh");
    }

    [Menu(NX_SHUTDOWN, NX_MENU, Icon = "fa-solid fa-power-off")]
    public static void Shutdown()
    {
        ExecuteCommand("/usr/bin/tkmm-shutdown.sh");
    }

    private static void ExecuteCommand(string command)
    {
        try {
            Process.Start("sh", command);
        }
        catch (Exception ex) {
            App.ToastError(new Exception($"Failed to execute command: {command}", ex));
        }
    }
}
#endif