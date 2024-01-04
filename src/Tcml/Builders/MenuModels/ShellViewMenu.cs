using Avalonia.Controls;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using Tcml.Attributes;
using Tcml.Core;
using Tcml.Core.Helpers.Operations;
using Tcml.Helpers;
using Tcml.Models.Mods;
using Tcml.ViewModels.Pages;

namespace Tcml.Builders.MenuModels;

public class ShellViewMenu
{
    [Menu("Exit", "File", "Alt + F4", "fa-solid fa-right-from-bracket")]
    public static void File_Exit()
    {
        // Handle exit cleanup here
        Environment.Exit(0);
    }

    [Menu("Import", "Mod", "Ctrl + I", "fa-solid fa-right-from-bracket")]
    public static async Task Mod_Import()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Mod");
        string? selectedFolder = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFolder)) {
            return;
        }

        Mod mod = Mod.FromFolder(selectedFolder);

        string modFolderName = DirectoryOperations
            .ToSafeName(mod.Name)
            .Replace(' ', '-')
            .ToLower();

        string outputModFolder = Path.Combine(Config.Shared.StorageFolder, "mods", modFolderName);
        DirectoryOperations.CopyDirectory(selectedFolder, outputModFolder);

        HomePageViewModel homePage
            = PageManager.Shared.Get<HomePageViewModel>(Page.Home);

        homePage.Mods.Add(mod);
        homePage.CurrentMod = homePage.Mods.Last();
    }
}
