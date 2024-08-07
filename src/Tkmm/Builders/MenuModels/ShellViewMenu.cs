﻿using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using FluentAvalonia.UI.Controls;
using Markdown.Avalonia.Full;
using System.Diagnostics;
using Tkmm.Attributes;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Helpers.Win32;
using Tkmm.Core.Models;
using Tkmm.Core.Models.Mods;
using Tkmm.Helpers;

namespace Tkmm.Builders.MenuModels;

public class ShellViewMenu
{
    [Menu("Export to SD Card", "File", "Ctrl + E", "fa-solid fa-sd-card")]
    public static async Task ExportToSdCard()
    {
        const string GAME_ID = "0100F2C0115B6000";

        FriendlyDriveInfo[] disks = DriveInfo.GetDrives()
            .Where(drive => {
                try {
                    return drive.DriveType == DriveType.Removable && drive.DriveFormat == "FAT32";
                }
                catch {
                    return false;
                }
            })
            .Select(x => new FriendlyDriveInfo(x))
            .ToArray();

        if (disks.Length == 0) {
            App.ToastError(new InvalidOperationException("""
                No removable disks found!
                """));

            return;
        }

        ContentDialog dialog = new() {
            Title = "Select SD Card",
            Content = new ComboBox {
                ItemsSource = disks,
                SelectedIndex = 0,
                DisplayMemberBinding = new Binding("DisplayName")
            },
            PrimaryButtonText = "Export",
            SecondaryButtonText = "Cancel"
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary || dialog.Content is not ComboBox selector) {
            return;
        }

        if (selector.SelectedItem is FriendlyDriveInfo drive) {
            await MergerOperations.Merge();

            try {
                string output = Path.Combine(drive.Drive.Name, "atmosphere", "contents", GAME_ID);
                DirectoryOperations.DeleteTargets(output, [TotkConfig.ROMFS, TotkConfig.EXEFS], recursive: true);
                DirectoryOperations.CopyDirectory(Config.Shared.MergeOutput, output, overwrite: true);
            }
            catch (Exception ex) {
                AppLog.Log(ex);
                App.ToastError(ex);
            }
        }
    }

    [Menu("Export Current Mod", "File", "Ctrl + Shift + E", "fa-solid fa-file-export")]
    public static async Task ExportTkcl()
    {
        if (ProfileManager.Shared.Current.Selected?.Mod is not Mod target) {
            return;
        }

        BrowserDialog dialog = new(BrowserMode.SaveFile, "Export TKCL", "TKCL:*.tkcl|All Files:*.*");
        string? selectedFile = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFile)) {
            return;
        }

        string folder = ProfileManager.GetModFolder(target);
        PackageBuilder.Package(folder, selectedFile);

        AppStatus.Set($"'{target.Name}' was exported successfully!", "fa-solid fa-circle-check",
                isWorkingStatus: false, temporaryStatusTime: 2.5);
    }

    [Menu("Cleanup Temporary Files", "File", "Ctrl + Shift + F6", "fa-solid fa-broom-wide", IsSeparator = true)]
    public static void ClearTempFolder()
    {
        string tempFolder = Path.Combine(Path.GetTempPath(), "tkmm");
        if (!Directory.Exists(tempFolder)) {
            return;
        }

        Directory.Delete(tempFolder, recursive: true);
        Directory.CreateDirectory(tempFolder);
        App.Toast(
            "The TKMM temporary files were succesfully deleted.", "Temporary Files Cleared", NotificationType.Success, TimeSpan.FromSeconds(3)
        );
    }

    [Menu("Exit", "File", "Alt + F4", "fa-solid fa-right-from-bracket", IsSeparator = true)]
    public static void Exit()
    {
        ProfileManager.Shared.Apply();
        Environment.Exit(0);
    }

    [Menu("Install File", "Mod", "Ctrl + I", "fa-solid fa-file-import")]
    public static async Task ImportModFile()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Open Mod File", "Supported Formats:*.tkcl;*.zip;*.rar;*.7z|TKCL:*.tkcl|Archives:*.tkcl;*.zip;*.rar;*.7z|All Files:*.*");
        string? selectedFile = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFile)) {
            return;
        }

        await ModHelper.Import(selectedFile);
    }

    [Menu("Install Folder", "Mod", "Ctrl + Shift + I", "fa-regular fa-folder-open")]
    public static async Task ImportModFolder()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Mod");
        string? selectedFolder = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFolder)) {
            return;
        }

        await ModHelper.Import(selectedFolder);
    }

    [Menu("Install from Argument", "Mod", "Ctrl + Alt + I", "fa-regular fa-keyboard")]
    public static async Task ImportArgument()
    {
        ContentDialog dialog = new() {
            Title = "Import Argument",
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
        await MergerOperations.Merge();
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

#if DEBUG
    [Menu("Open Mod Folder", "Debug", "Alt + O", "fa-solid fa-folder-tree")]
    public static void OpenModFolder()
    {
        if (ProfileManager.Shared.Current.Selected?.Mod is not Mod target) {
            return;
        }

        string folder = ProfileManager.GetModFolder(target);
        if (OperatingSystem.IsWindows()) {
            Process.Start("explorer.exe", folder);
        }
        else {
            App.ToastError(new InvalidOperationException("""
                This operations is only supported on Windows
                """));
        }
    }
#endif

    [Menu("Help", "Help", "F1", "fa-solid fa-circle-question")]
    public static Task GoToHelp()
    {
        Process.Start(new ProcessStartInfo("https://totkmods.github.io/tkmm/docs/index.html") {
            UseShellExecute = true
        });

        return Task.CompletedTask;
    }

    [Menu("Check for Update", "Help", "Ctrl + U", "fa-solid fa-cloud-arrow-up")]
    public static async Task CheckForUpdate()
    {
        if (!(await AppManager.HasUpdate()).Result) {
            await new ContentDialog {
                Title = "Check for Updates",
                Content = "Software up to date.",
                PrimaryButtonText = "OK"
            }.ShowAsync();

            return;
        }

       await App.PromptUpdate();
    }

    [Menu("Download Assets", "Help", "Ctrl + Shift + U", "fa-solid fa-screwdriver-wrench")]
    public static async Task DownloadAssets()
    {
        await AssetHelper.Download();
    }

    [Menu("Create Desktop Shortcuts", "Help", "Ctrl + Alt + L", "fa-solid fa-link")]
    public static Task CreateDesktopShortcuts()
    {
        AppManager.CreateDesktopShortcuts();
        return Task.CompletedTask;
    }

    [Menu("About", "Help", "F12", "fa-solid fa-circle-info", IsSeparator = true)]
    public static async Task About()
    {
        await using Stream aboutFileStream = AssetLoader.Open(new Uri("avares://Tkmm/Assets/About.md"));
        string contents = await new StreamReader(aboutFileStream).ReadToEndAsync();

        // Replace version info
        contents = contents.Replace("@@version@@", App.Version);

        TaskDialog dialog = new() {
            XamlRoot = App.XamlRoot,
            Title = "About",
            Content = new MarkdownScrollViewer {
                Markdown = contents,
                Styles = {
                    new StyleInclude(StyleHelper.MarkdownStyleUri) {
                        Source = StyleHelper.MarkdownStyleUri
                    }
                }
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
