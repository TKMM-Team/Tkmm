using MenuFactory.Abstractions.Attributes;
using Tkmm.Actions;
using Tkmm.Builders;
using Tkmm.Core;
using Tkmm.Helpers;
using static Tkmm.Core.Localization.StringResources_Menu;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public class ModMenuModel
{
    [Menu(MOD_EXPORT, MOD_MENU, InputGesture = "Ctrl + Shift + E", Icon = "fa-solid fa-file-export")]
    public static Task Export()
    {
        return ModActions.Instance.ExportMod();
    }

    [Menu(MOD_CONFIGURE_OPTIONS, MOD_MENU, InputGesture = "F4", Icon = "fa-sliders")]
    public static Task ConfigureOptions()
    {
        return ModActions.Instance.ConfigureModOptions();
    }

    [Menu(MOD_REMOVE_FROM_PROFILE, MOD_MENU, InputGesture = "Ctrl + Delete", Icon = "fa-regular fa-trash", IsSeparator = true)]
    public static Task RemoveFromProfile()
    {
        return ModActions.Instance.RemoveModFromProfile();
    }

    [Menu(MOD_UNINSTALL, MOD_MENU, InputGesture = "Ctrl + Shift + Delete", Icon = "fa-solid fa-trash")]
    public static Task Uninstall()
    {
        return ModActions.Instance.UninstallMod();
    }

    [Menu(MOD_EDIT_EXPORT_LOCATIONS, MOD_MENU, InputGesture = "Ctrl + L", Icon = "fa-regular fa-pen-to-square", IsSeparator = true)]
    public static async Task EditExportLocations()
    {
        // TODO: Abstract this to somewhere else
        await ExportLocationControlBuilder.Edit(TKMM.Config.ExportLocations);
    }

    [Menu(MOD_MERGE, MOD_MENU, InputGesture = "Ctrl + M", Icon = "fa-solid fa-list-check")]
    public static Task MergeMods()
    {
        return MergerOperations.Merge();
    }
}