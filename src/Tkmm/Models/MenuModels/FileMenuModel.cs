using MenuFactory.Abstractions.Attributes;
using Tkmm.Actions;
using static Tkmm.Core.Localization.StringResources_Menu;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public class FileMenuModel
{
    [Menu(FILE_INSTALL_FILE, FILE_MENU, InputGesture = "Ctrl + I", Icon = "fa-solid fa-file-import")]
    public static Task InstallFile(CancellationToken ct = default)
    {
        return ImportActions.Instance.ImportFromFile(ct);
    }

    [Menu(FILE_INSTALL_FOLDER, FILE_MENU, InputGesture = "Ctrl + Shift + I", Icon = "fa-regular fa-folder-open")]
    public static Task InstallFolder(CancellationToken ct = default)
    {
        return ImportActions.Instance.ImportFromFolder(ct);
    }

    [Menu(FILE_INSTALL_ARGUMENT, FILE_MENU, InputGesture = "Ctrl + Alt + I", Icon = "fa-regular fa-keyboard")]
    public static Task ImportArgument(CancellationToken ct = default)
    {
        return ImportActions.Instance.ImportFromArgument(ct);
    }

    [Menu(FILE_CLEAR_TEMP_FILES, FILE_MENU, InputGesture = "Ctrl + Shift + F6", Icon = "fa-solid fa-broom-wide", IsSeparator = true)]
    public static Task ClearTempFolder()
    {
        return SystemActions.Instance.CleanupTempFolder();
    }

    [Menu(FILE_EXIT, FILE_MENU, InputGesture = "Alt + F4", Icon = "fa-solid fa-right-from-bracket", IsSeparator = true)]
    public static Task Exit()
    {
        return SystemActions.Instance.SoftClose();
    }
}