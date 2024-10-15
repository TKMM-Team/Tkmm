using MenuFactory.Abstractions.Attributes;
using Tkmm.Actions;
using static Tkmm.Core.Localization.StringResources_Menu;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public sealed class ToolsMenuModel
{
    [Menu(TOOLS_EXPORT_TO_SD_CARD, TOOLS_MENU, InputGesture = "Ctrl + E", Icon = TkIcons.SD_CARD)]
    public static Task ExportToSdCard()
    {
        return MergeActions.Instance.ExportToSdCard();
    }
}