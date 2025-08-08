#if SWITCH

using System.Diagnostics;
using Tkmm.Attributes;
using Tkmm.ViewModels;

namespace Tkmm.Models.MenuModels;

public class NxMenuModel
{
    [TkMenu(TkLocale.Menu_NxReboot, TkLocale.Menu_Nx, Icon = "fa-solid fa-rotate")]
    public static void Reboot()
    {
        ExecuteCommand("/usr/bin/tkmm-reboot");
    }

    [TkMenu(TkLocale.Menu_NxShutdown, TkLocale.Menu_Nx, Icon = "fa-solid fa-power-off")]
    public static void Shutdown()
    {
        ExecuteCommand("/usr/bin/tkmm-shutdown");
    }
    
    [TkMenu(TkLocale.Menu_NxReboot2Config, TkLocale.Menu_Nx, Icon = "fa-solid fa-rocket")]
    public static void RebootMenu()
    {
        ShellViewModel.Shared.ShowR2CMenu();
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