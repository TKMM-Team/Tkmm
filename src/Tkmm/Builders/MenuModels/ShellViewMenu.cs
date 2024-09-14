using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using FluentAvalonia.UI.Controls;
using Markdown.Avalonia.Full;
using MenuFactory.Abstractions.Attributes;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models;
using Tkmm.Core.Models.Mods;
using Tkmm.Helpers;
using Tkmm.ViewModels.Pages;
using TotkCommon;
using TotkCommon.Components;
using WindowsOperations = Tkmm.Core.Helpers.Windows.WindowsOperations;

namespace Tkmm.Builders.MenuModels;

public class ShellViewMenu
{
    [Menu("Install File", "File", InputGesture = "Ctrl + I", Icon = "fa-solid fa-file-import")]
    public static async Task ImportModFile()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Open Mod File", "Supported Formats:*.tkcl;*.zip;*.rar;*.7z|TKCL:*.tkcl|Archives:*.tkcl;*.zip;*.rar;*.7z|All Files:*.*");
        string? selectedFile = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFile)) {
            return;
        }

        await ModHelper.Import(selectedFile);
    }

    [Menu("Install Folder", "File", InputGesture = "Ctrl + Shift + I", Icon = "fa-regular fa-folder-open")]
    public static async Task ImportModFolder()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Mod");
        string? selectedFolder = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFolder)) {
            return;
        }

        await ModHelper.Import(selectedFolder);
    }

    [Menu("Install from Argument", "File", InputGesture = "Ctrl + Alt + I", Icon = "fa-regular fa-keyboard")]
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

    [Menu("Cleanup Temporary Files", "File", InputGesture = "Ctrl + Shift + F6", Icon = "fa-solid fa-broom-wide", IsSeparator = true)]
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

    [Menu("Exit", "File", InputGesture = "Alt + F4", Icon = "fa-solid fa-right-from-bracket", IsSeparator = true)]
    public static void Exit()
    {
        ProfileManager.Shared.Apply();
        Environment.Exit(0);
    }

    [Menu("Export", "Mod", InputGesture = "Ctrl + Shift + E", Icon = "fa-solid fa-file-export")]
    public static async Task ExportTkcl()
    {
        if (ProfileManager.Shared.Current.Selected?.Mod is not Mod target) {
            return;
        }

        BrowserDialog dialog = new(BrowserMode.SaveFile, "Export TKCL", "TKCL:*.tkcl|All Files:*.*", target.Name);
        string? selectedFile = await dialog.ShowDialog();

        if (string.IsNullOrEmpty(selectedFile)) {
            return;
        }

        string folder = ProfileManager.GetModFolder(target);
        PackageBuilder.Package(folder, selectedFile);

        AppStatus.Set($"'{target.Name}' was exported successfully!", "fa-solid fa-circle-check",
                isWorkingStatus: false, temporaryStatusTime: 2.5);
    }

    [Menu("Configure Options", "Mod", InputGesture = "F4", Icon = "fa-sliders")]
    public static void ConfigureOptions()
    {
        if (ProfileManager.Shared.Current.Selected?.Mod is Mod mod) {
            mod.IsEditingOptions = !mod.IsEditingOptions;
        }
    }

    [Menu("Remove from Profile", "Mod", InputGesture = "Ctrl + Delete", Icon = "fa-regular fa-trash", IsSeparator = true)]
    public static void RemoveFromProfile()
    {
        if (ProfileManager.Shared.Current.Selected?.Mod is Mod mod) {
            ProfileManager.Shared.Current.Mods.Remove(mod);
        }
    }

    [Menu("Uninstall", "Mod", InputGesture = "Ctrl + Shift + Delete", Icon = "fa-solid fa-trash")]
    public static async Task Uninstall()
    {
        await PageManager.Shared.Get<ProfilesPageViewModel>(Page.Profiles)
            .UninstallCommand
            .ExecuteAsync(ProfileManager.Shared.Current.Selected?.Mod);
    }

    [Menu("Edit Export Locations", "Mod", InputGesture = "Ctrl + L", Icon = "fa-regular fa-pen-to-square", IsSeparator = true)]
    public static async Task EditExportLocations()
    {
        await ExportLocationControlBuilder.Edit(Config.Shared.ExportLocations);
    }

    [Menu("Merge", "Mod", InputGesture = "Ctrl + M", Icon = "fa-solid fa-list-check")]
    public static async Task MergeMods()
    {
        await MergerOperations.Merge();
    }

    [Menu("Export to SD Card", "Tools", InputGesture = "Ctrl + E", Icon = "fa-solid fa-sd-card")]
    public static async Task ExportToSdCard()
    {
        const string gameId = "0100F2C0115B6000";

        FriendlyDriveInfo[] disks = DriveInfo.GetDrives()
            .Where(driveInfo => {
                try {
                    return driveInfo is { DriveType: DriveType.Removable, DriveFormat: "FAT32" };
                }
                catch {
                    return false;
                }
            })
            .Select(x => new FriendlyDriveInfo(x))
            .ToArray();

        if (disks.Length == 0) {
            App.ToastError(new InvalidOperationException(
                "No removable disks found!"
            ));

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
                string output = Path.Combine(drive.Drive.Name, "atmosphere", "contents", gameId);
                DirectoryOperations.DeleteTargets(output, [TotkConfig.ROMFS, TotkConfig.EXEFS], recursive: true);
                DirectoryOperations.CopyDirectory(Config.Shared.MergeOutput, output, overwrite: true);
            }
            catch (Exception ex) {
                AppLog.Log(ex);
                App.ToastError(ex);
            }
        }
    }

    [Menu("Check Dump Integrity", "Tools", Icon = "fa-regular fa-arrow-progress", IsSeparator = true)]
    public static async Task CheckDumpIntegrity()
    {
        AppStatus.Set($"Checking Dump Integrity",
            "fa-solid fa-file-magnifying-glass",
            isWorkingStatus: true
        );

        TotkDumpResults results;

        try {
            results = await Task.Run(async () => TotkDump.CheckIntegrity(
                Totk.Config.GamePath,
                await DumpChecksumTableHelper.DownloadChecksumTable(),
                CheckDumpIntegrityUpdateCallback
            ));
        }
        catch (Exception ex) {
            App.ToastError(new Exception($"Dump Integrity Check Failed", ex));
            return;
        }

        AppStatus.Set($"Dump Integrity Check Completed",
            "fa-circle-check",
            isWorkingStatus: false,
            temporaryStatusTime: 3.5
        );

        string resultContent = $"""
            {(results.MissingFiles <= 0 ? 0 : results.MissingFiles)} missing files recorded.
            {results.BadFiles.Count} corrupt files recorded.{(results.BadFiles.Count > 0 ? "\n- `" : string.Empty)}{string.Join("`\n- `", results.BadFiles)}{(results.BadFiles.Count > 0 ? '`' : string.Empty)}
            {results.ExtraFiles.Count} extra files recorded.{(results.ExtraFiles.Count > 0 ? "\n- `" : string.Empty)}{string.Join("`\n- `", results.ExtraFiles)}{(results.ExtraFiles.Count > 0 ? '`' : string.Empty)}
            """;

        ContentDialog dialog = new() {
            Title = "Dump Integrity Check Results",
            Content = new StackPanel {
                Children = {
                    new TextBlock {
                        Margin = new(0, 0, 0, 5),
                        Text = results.IsCompleteDump ? "Verified Complete Game Dump" : "Invalid Game Dump",
                        FontSize = 16
                    },
                    new Button {
                        Margin = new(0, 0, 0, 5),
                        Padding = new(5, 2),
                        Content = "Copy Report",
                        Command = new AsyncRelayCommand(async () => {
                            if (App.XamlRoot?.Clipboard?.SetTextAsync(resultContent) is Task task) {
                                await task;
                            }
                        })
                    },
                    new TextBlock {
                        TextWrapping = TextWrapping.WrapWithOverflow,
                        Text = resultContent
                    },
                }
            },
            IsSecondaryButtonEnabled = false,
            PrimaryButtonText = "OK",
            IsPrimaryButtonEnabled = true,
            DefaultButton = ContentDialogButton.Primary
        };

        await dialog.ShowAsync();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckDumpIntegrityUpdateCallback(int i, int length)
    {
        AppStatus.Set($"Checking {i}/{length}...",
            "fa-regular fa-arrow-progress",
            isWorkingStatus: false,
            logLevel: LogLevel.None
        );
    }

    [Menu("Show/Hide Console", "View", InputGesture = "Ctrl + F11", Icon = "fa-solid fa-terminal")]
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
    [Menu("Open Mod Folder", "Debug", InputGesture = "Alt + O", Icon = "fa-solid fa-folder-tree")]
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

    [Menu("Help", "Help", InputGesture = "F1", Icon = "fa-solid fa-circle-question")]
    public static Task GoToHelp()
    {
        Process.Start(new ProcessStartInfo("https://tkmm.org/docs/using-mods/") {
            UseShellExecute = true
        });

        return Task.CompletedTask;
    }

    [Menu("Check for Update", "Help", InputGesture = "Ctrl + U", Icon = "fa-solid fa-cloud-arrow-up")]
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

    [Menu("Download Assets", "Help", InputGesture = "Ctrl + Shift + U", Icon = "fa-solid fa-screwdriver-wrench")]
    public static async Task DownloadAssets()
    {
        await AssetHelper.Download();
    }

    [Menu("Create Desktop Shortcuts", "Help", InputGesture = "Ctrl + Alt + L", Icon = "fa-solid fa-link")]
    public static Task CreateDesktopShortcuts()
    {
        AppManager.CreateDesktopShortcuts();
        return Task.CompletedTask;
    }

    [Menu("About", "Help", InputGesture = "F12", Icon = "fa-solid fa-circle-info", IsSeparator = true)]
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
