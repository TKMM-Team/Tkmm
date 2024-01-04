using Avalonia.Controls;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using System.Text.Json;
using Tcml.Attributes;
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

        string jsonFilePath = Path.Combine(selectedFolder, "info.json");
        string jsonContent = File.ReadAllText(jsonFilePath);

        ModInfo modInfo = JsonSerializer.Deserialize<ModInfo>(jsonContent)
            ?? throw new InvalidOperationException(
                "Could not parse ModInfo");

        HomePageViewModel homePage
            = PageManager.Shared.Get<HomePageViewModel>(Page.Home);
        homePage.Description = modInfo.Description;
    }
}
