using Avalonia.Controls;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using Tkmm.Attributes;
using Tkmm.Core.Components;
using Tkmm.Core.Models.Mods;
using Tkmm.Helpers;
using Tkmm.ViewModels.Pages;

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
    public static async Task Mod_Import()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Mod");
        string? selectedFolder = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFolder)) {
            return;
        }

        ModManager.Shared.Import(selectedFolder);
    }

    [Menu("Merge", "Mod", "Ctrl + M", "fa-solid fa-code-merge")]
    public static async Task Mod_Merge()
    {
        await ModManager.Shared.Merge();
    }
}
