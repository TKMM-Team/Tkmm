using MenuFactory.Abstractions.Attributes;
using Tkmm.Actions;
using static Tkmm.Core.Localization.StringResources_Menu;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public sealed class HelpMenuModel
{
    [Menu(HELP_MENU, HELP_MENU, InputGesture = "F1", Icon = TkIcons.CIRCLE_QUESTION)]
    public static Task Help()
    {
        return SystemActions.Instance.OpenDocumentationWebsite();
    }

    [Menu(HELP_CHECK_FOR_UPDATES, HELP_MENU, InputGesture = "Ctrl + U", Icon = TkIcons.CLOUD_ARROW_UP)]
    public static Task CheckForUpdates()
    {
        return SystemActions.Instance.CheckForUpdates(isAutoCheck: false);
    }

    [Menu(HELP_ABOUT, HELP_MENU, InputGesture = "F12", Icon = TkIcons.CIRCLE_INFO, IsSeparator = true)]
    public static Task About()
    {
        return SystemActions.Instance.ShowAboutDialog();
    }
}