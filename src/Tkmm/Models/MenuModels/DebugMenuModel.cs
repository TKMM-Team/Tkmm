using Tkmm.Actions;
using Tkmm.Attributes;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public sealed class DebugMenuModel
{
    [TkMenu(TkLocale.Menu_DebugOpenModFolder, TkLocale.Menu_Debug, InputGesture = "Alt + O", Icon = TkIcons.FOLDER_TREE)]
    public static Task OpenModFolder()
    {
        return ModActions.Instance.OpenModFolder();
    }
}