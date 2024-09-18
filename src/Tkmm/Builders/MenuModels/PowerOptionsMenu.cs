using System.Diagnostics;
using MenuFactory.Abstractions;
using MenuFactory.Abstractions.Attributes;

namespace Tkmm.Builders.MenuModels;

public class PowerOptionsMenu
{
    [Menu("Reboot", "Power Options", Icon = "fa-solid fa-rotate")]
    public static void Reboot()
    {
        ExecuteCommand("/usr/bin/tkmm-reboot.sh");
    }

    [Menu("Shutdown", "Power Options", Icon = "fa-solid fa-power-off")]
    public static void Shutdown()
    {
        ExecuteCommand("/usr/bin/tkmm-shutdown.sh");
    }

    private static void ExecuteCommand(string command)
    {
        try
        {
            Process.Start("sh", command);
        }
        catch (Exception ex)
        {
            App.ToastError(new Exception($"Failed to execute command: {command}", ex));
        }
    }
}