using Tkmm.Actions;
using Tkmm.Attributes;
using Tkmm.Core;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public sealed class ToolsMenuModel
{
#if !SWITCH
    [TkMenu(TkLocale.Menu_ToolsExportToSdCard, TkLocale.Menu_Tools, InputGesture = "Ctrl + E", Icon = TkIcons.SD_CARD)]
    public static Task ExportToSdCard()
    {
        return MergeActions.Instance.ExportToSdCard();
    }
    
    [TkMenu(TkLocale.Menu_DebugOpenMergedOutput, TkLocale.Menu_Tools, InputGesture = "Ctrl + Shift + O", Icon = TkIcons.FOLDER_TREE)]
    public static Task OpenMergedOutput()
    {
        return MergeActions.Instance.OpenMergedOutput();
    }
#endif
    
    [TkMenu(TkLocale.Menu_ToolsEmptyMergeOutput, TkLocale.Menu_Tools, InputGesture = "Alt + Delete", Icon = "fa-solid fa-trash-xmark")]
    public static void EmptyMergeOutput()
    {
        TKMM.EmptyMergeOutput(TKMM.MergedOutputFolder);
    }
}