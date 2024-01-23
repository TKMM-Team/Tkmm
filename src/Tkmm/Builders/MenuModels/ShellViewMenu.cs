using Avalonia.Controls;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using Tkmm.Attributes;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers.Win32;

namespace Tkmm.Builders.MenuModels;

public class ShellViewMenu
{
    [Menu("Exit", "File", "Alt + F4", "fa-solid fa-right-from-bracket")]
    public static void File_Exit()
    {
        // Handle exit cleanup here
        Environment.Exit(0);
    }

    [Menu("Import", "Mod", "Ctrl + I", "fa-solid fa-file-import")]
    public static async Task ModImport()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Mod");
        string? selectedFolder = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFolder)) {
            return;
        }

        ModManager.Shared.Import(selectedFolder);
    }

    [Menu("Merge", "Mod", "Ctrl + M", "fa-solid fa-code-merge")]
    public static async Task ModMerge()
    {
        AppStatus.Set("Merging", "fa-solid fa-code-merge", isWorkingStatus: true);
        await ModManager.Shared.Merge();
        AppStatus.Set("Merge Completed", "fa-solid fa-list-check", isWorkingStatus: false, 1.5);
    }

    [Menu("Show/Hide Console", "View", "Ctrl + F11", "fa-solid fa-terminal")]
    public static void ShowHideConsole()
    {
        if (OperatingSystem.IsWindows()) {
            WindowsOperations.SwapWindowMode();
            App.Focus();
        }
        else {
            AppStatus.Set("This action is only supported on Win32 platforms", "fa-brands fa-windows",
                isWorkingStatus: false, temporaryStatusTime: 1.5);
        }
    }
}
