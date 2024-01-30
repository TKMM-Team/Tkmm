using Avalonia.Controls;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using FluentAvalonia.UI.Controls;
using Markdown.Avalonia.Full;
using Tkmm.Attributes;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Win32;
using Tkmm.Helpers;

namespace Tkmm.Builders.MenuModels;

public class ShellViewMenu
{
    [Menu("Exit", "File", "Alt + F4", "fa-solid fa-right-from-bracket")]
    public static void File_Exit()
    {
        ModManager.Shared.Apply();
        Environment.Exit(0);
    }

    [Menu("Import File", "Mod", "Ctrl + I", "fa-solid fa-file-import")]
    public static async Task ImportModFile()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Open Mod File", "TKCL:*.tkcl|All Archives:*.tkcl;*.zip;*.rar;*.7z|All Files:*.*");
        string? selectedFile = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFile)) {
            return;
        }

        await ModHelper.Import(selectedFile);
    }

    [Menu("Import Folder", "Mod", "Ctrl + Shift + I", "fa-regular fa-folder-open")]
    public static async Task ImportModFolder()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Mod");
        string? selectedFolder = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFolder)) {
            return;
        }

        await ModHelper.Import(selectedFolder);
    }

    [Menu("Import Argument", "Mod", "Ctrl + Alt + I", "fa-regular fa-folder-open")]
    public static async Task ImportArgument()
    {
        ContentDialog dialog = new() {
            Title = "Improt Argument",
            Content = new TextBox {
                Watermark = "Argument (File, Folder, URL, Mod ID)"
            },
            PrimaryButtonText = "Import",
            SecondaryButtonText = "Cancel",
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary) {
            return;
        }

        if (dialog.Content is TextBox tb && tb.Text is not null) {
            await ModHelper.Import(tb.Text.Replace("\"", string.Empty));
        }
    }

    [Menu("Merge", "Mod", "Ctrl + M", "fa-solid fa-code-merge", IsSeparator = true)]
    public static async Task MergeMods()
    {
        await ModManager.Shared.Merge();
    }

    [Menu("Show/Hide Console", "View", "Ctrl + F11", "fa-solid fa-terminal")]
    public static void ShowHideConsole()
    {
        if (OperatingSystem.IsWindows()) {
            WindowsOperations.SwapWindowMode();
            App.Focus();
        }
        else {
            AppStatus.Set("This action is only supported on Win32 platforms", "fa-brands fa-windows",
                isWorkingStatus: false, temporaryStatusTime: 1.5);
        }
    }

    [Menu("Check for Update", "Help", "Ctrl + U", "fa-solid fa-cloud-arrow-up")]
    public static async Task CheckForUpdate()
    {
        if (!await AppManager.HasUpdate()) {
            await new ContentDialog {
                Title = "Check for Updates",
                Content = "Software up to date.",
                PrimaryButtonText = "OK"
            }.ShowAsync();

            return;
        }

        ContentDialog dialog = new() {
            Title = "Update",
            Content = """
                An update is availible.
                
                Would you like to close your current session and open the launcher?
                """,
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "Cancel"
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary) {
            await Task.Run(async () => {
                await AppManager.UpdateLauncher();
                AppManager.StartLauncher();
            });

            Environment.Exit(0);
        }
    }

    [Menu("Download Dependencies", "Help", "Ctrl + Shift + U", "fa-solid fa-screwdriver-wrench")]
    public static async Task DownloadDependencies()
    {
        await ToolHelper.DownloadDependencies();
        await AssetHelper.Download();
    }

    [Menu("About", "Help", "F12", "fa-solid fa-circle-info", IsSeparator = true)]
    public static async Task About()
    {
        string aboutFile = Path.Combine(Config.Shared.StaticStorageFolder, "Readme.md");

        TaskDialog dialog = new() {
            XamlRoot = App.XamlRoot,
            Title = "About",
            Content = new MarkdownScrollViewer {
                Markdown = File.Exists(aboutFile) ? File.ReadAllText(aboutFile) : "Invalid Installation"
            },
            Buttons = [
                new TaskDialogButton {
                    Text = "OK"
                }
            ]
        };

        await dialog.ShowAsync();
    }
}
