using MenuFactory.Abstractions.Attributes;
using Tkmm.Actions;
using static Tkmm.Core.Localization.StringResources_Menu;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public sealed class DebugMenuModel
{
    [Menu(DEBUG_OPEN_MOD_FOLDER, DEBUG_MENU, InputGesture = "Alt + O", Icon = TkIcons.FOLDER_TREE)]
    public static Task OpenModFolder()
    {
        return ModActions.Instance.OpenModFolder();
    }
    
    [Menu(DEBUG_OPEN_MERGED_OUTPUT, DEBUG_MENU, InputGesture = "Alt + Shift + O", Icon = TkIcons.FOLDER_TREE)]
    public static Task OpenMergedOutput()
    {
        return MergeActions.Instance.OpenMergedOutput();
    }
}