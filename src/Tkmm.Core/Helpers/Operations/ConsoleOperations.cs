using Tkmm.Core.Helpers.Windows;
using WindowsOperations = Tkmm.Core.Helpers.Windows.WindowsOperations;

namespace Tkmm.Core.Helpers.Operations;

public static class ConsoleOperations
{
    public static void WaitOnFailure(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex);
        Console.ResetColor();

        if (OperatingSystem.IsWindows()) {
            WindowsOperations.SetWindowMode(WindowMode.Visible);
        }

        Console.WriteLine("Press any key to continue . . .");
        Console.ReadKey();
    }
}
