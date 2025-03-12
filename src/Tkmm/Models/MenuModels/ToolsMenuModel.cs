using Tkmm.Actions;
using Tkmm.Attributes;

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
}