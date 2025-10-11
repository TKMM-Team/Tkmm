using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Dialogs;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Actions;

public sealed partial class ImportActions : GuardedActionGroup<ImportActions>
{
    public static readonly FilePickerFileType SupportedFormats = new("Supported Formats") {
        Patterns = ["*.tkcl","*.zip","*.rar","*.7z"]
    };
    
    public static readonly FilePickerFileType TkclFormat = new("TKCL") {
        Patterns = ["*.tkcl"]
    };

    protected override string ActionGroupName { get; } = nameof(ImportActions).Humanize();

    [RelayCommand]
    public Task ImportFromFile(CancellationToken ct = default)
        => ImportFromFile(null, ct);
    
    public async Task<bool> ImportFromFile(TkModContext? context, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return false;
        }

        var results = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = "Import from File",
            AllowMultiple = true,
            FileTypeFilter = [
                SupportedFormats,
                TkclFormat,
                FilePickerFileTypes.All
            ]
        });

        if (results.Count <= 0) {
            return false;
        }

        foreach (var targetFile in results) {
            try {
                TkStatus.Set(Locale[TkLocale.Status_Importing, targetFile.Name], TkIcons.PROGRESS);
                await using var stream = await targetFile.OpenReadAsync();
                if (await TKMM.Install(targetFile.Name, stream, context, ct: ct) is TkMod result) {
                    TkStatus.SetTemporary(Locale[TkLocale.Status_Imported, result.Name], TkIcons.CIRCLE_CHECK);
                    return true;
                }
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, "An error occured while importing the file '{TargetFile}'.", targetFile?.Name);
                await ErrorDialog.ShowAsync(ex);
            }
        }
        
        return false;
    }

    [RelayCommand]
    public Task ImportFromFolder(CancellationToken ct = default)
        => ImportFromFolder(null, ct);
    
    public async Task<bool> ImportFromFolder(TkModContext? context, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return false;
        }

        var results = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = "Import from Folder",
            AllowMultiple = true
        });

        if (results.Count <= 0) {
            return false;
        }

        foreach (var targetFolder in results) {
            try {
                if (targetFolder.TryGetLocalPath() is not string folder) {
                    continue;
                }
                
                TkStatus.Set(Locale[TkLocale.Status_Importing, folder], TkIcons.GEAR_FOLDER);
                if (await TKMM.Install(folder, context: context, ct: ct) is TkMod result) {
                    TkStatus.SetTemporary(Locale[TkLocale.Status_Imported, result.Name], TkIcons.CIRCLE_CHECK);
                    return true;
                }
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, "An error occured while importing the folder '{TargetFolder}'.", targetFolder?.Name);
                await ErrorDialog.ShowAsync(ex);
            }
        }
        
        return false;
    }

    [RelayCommand]
    public Task ImportFromArgument(CancellationToken ct = default)
        => ImportFromArgument(null, ct);

    public async Task<bool> ImportFromArgument(TkModContext? context, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return false;
        }
        
        ContentDialog dialog = new() {
            Title = "Import from Argument",
            Content = new TextBox {
                Watermark = "Argument (File, Folder, URL, Mod ID)"
            },
            PrimaryButtonText = "Import",
            SecondaryButtonText = "Cancel",
        };
        
        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return false;
        }
        
        if (dialog.Content is TextBox { Text: not null } textBox) {
            string argument = textBox.Text;
            try {
                TkStatus.Set(Locale[TkLocale.Status_Importing, argument], TkIcons.GEAR_FOLDER);
                if (await TKMM.Install(argument, context: context, ct: ct) is TkMod result) {
                    TkStatus.SetTemporary(Locale[TkLocale.Status_Imported, result.Name], TkIcons.CIRCLE_CHECK);
                    return true;
                }
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, "An error occured while importing the argument '{Argument}'.", argument);
                await ErrorDialog.ShowAsync(ex);
            }
        }

        return false;
    }

    [RelayCommand]
    public async Task Update(CancellationToken ct = default)
    {
        if (TKMM.ModManager.CurrentProfile?.Selected is not TkProfileMod target) {
            return;
        }
        
        TaskDialog dialog = new() {
            Header = Locale[TkLocale.System_Popup_UpdateMod_Title],
            SubHeader = Locale[TkLocale.System_Popup_UpdateMod, target.Mod.Name],
            IconSource = new SymbolIconSource {
                Symbol = Symbol.Upload
            },
            Buttons = {
                new TaskDialogButton(Locale[TkLocale.Word_Argument], "arg"),
                new TaskDialogButton(Locale[TkLocale.Word_Folder], "folder"),
                new TaskDialogButton(Locale[TkLocale.Word_File], "file"),
            },
            XamlRoot = App.XamlRoot
        };

        TkModContext context = new(target.Mod.Id);

        Func<TkModContext, CancellationToken, Task<bool>>? exec = await dialog.ShowAsync() switch {
            "arg" => ImportFromArgument,
            "folder" => ImportFromFolder,
            "file" => ImportFromFile,
            _ => null
        };

        if (exec is null) {
            return;
        }
        
        TkStatus.Set(Locale[TkLocale.Status_Updating, target.Mod.Name], TkIcons.GEAR_FOLDER);
        if (await exec(context, ct)) {
            TkStatus.SetTemporary(Locale[TkLocale.Status_Updated, target.Mod.Name], TkIcons.CIRCLE_CHECK);
        }
    }
}