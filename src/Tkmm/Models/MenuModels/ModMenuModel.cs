using Tkmm.Actions;
using Tkmm.Attributes;
using Tkmm.Core;
using Tkmm.Helpers;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public class ModMenuModel
{
    [TkMenu(TkLocale.Menu_ModExportFile, TkLocale.Menu_Mod, InputGesture = "Ctrl + Shift + E", Icon = "fa-solid fa-file-export")]
    public static Task Export()
    {
        return ModActions.Instance.ExportMod();
    }
    
    [TkMenu(TkLocale.Menu_ModExportFolder, TkLocale.Menu_Mod, InputGesture = "Ctrl + Shift + B", Icon = "fa-solid fa-folder-gear")]
    public static Task ExportModRomfs()
    {
        return ModActions.Instance.ExportModRomfs();
    }

    [TkMenu(TkLocale.Menu_ModExportProject, TkLocale.Menu_Mod, InputGesture = "Ctrl + Shift + P", Icon = "fa-solid fa-diagram-project")]
    public static Task ExportModProject()
    {
        return ModActions.Instance.ExportModAsProject();
    }

    [TkMenu(TkLocale.Menu_ModConfigureOptions, TkLocale.Menu_Mod, InputGesture = "F4", Icon = "fa-sliders")]
    public static Task ConfigureOptions()
    {
        return ModActions.Instance.ConfigureModOptions();
    }

    [TkMenu(TkLocale.Menu_ModRemoveFromProfile, TkLocale.Menu_Mod, InputGesture = "Ctrl + Delete", Icon = "fa-regular fa-trash", IsSeparator = true)]
    public static Task RemoveFromProfile()
    {
        return ModActions.Instance.RemoveModFromProfile();
    }

    [TkMenu(TkLocale.Menu_ModUninstall, TkLocale.Menu_Mod, InputGesture = "Ctrl + Shift + Delete", Icon = "fa-solid fa-trash")]
    public static Task Uninstall()
    {
        return ModActions.Instance.UninstallMod();
    }

#if !SWITCH
    [TkMenu(TkLocale.EditExportLocations_Title, TkLocale.Menu_Mod, InputGesture = "Ctrl + L", Icon = "fa-regular fa-pen-to-square", IsSeparator = true)]
    public static async Task EditExportLocations()
    {
        await ExportLocationsHelper.OpenEditorDialog(TKMM.Config.ExportLocations);
    }
#endif

    [TkMenu(TkLocale.Menu_ModMerge, TkLocale.Menu_Mod, InputGesture = "Ctrl + M", Icon = "fa-solid fa-list-check")]
    public static Task MergeMods()
    {
        return MergeActions.Instance.Merge();
    }
}