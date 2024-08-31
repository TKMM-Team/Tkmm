using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using FluentAvalonia.UI.Controls;
using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Components.ModReaders;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;

namespace Tkmm.ViewModels.Pages;

public partial class PackagingPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _exportPath = string.Empty;

    [ObservableProperty]
    private Mod _mod = new();

    [ObservableProperty]
    private string _sourceFolder = string.Empty;

    [RelayCommand]
    private async Task EditContributors(ContentControl content)
    {
        content.DataContext = this;

        ContentDialog dialog = new() {
            Title = "Contributors",
            Content = content,
            IsSecondaryButtonEnabled = false,
            PrimaryButtonText = "OK"
        };

        await dialog.ShowAsync();
        dialog.Content = null;
    }

    [RelayCommand]
    private async Task BrowseExportPath()
    {
        BrowserDialog dialog = new(BrowserMode.SaveFile, "Export Location", "TKCL Files:*.tkcl");
        if (await dialog.ShowDialog() is string result) {
            ExportPath = result;
        }
    }

    [RelayCommand]
    private async Task BrowseSourceFolder()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Source Folder");
        if (await dialog.ShowDialog() is string result) {
            SourceFolder = result;
        }
    }

    [RelayCommand]
    private static async Task BrowseThumbnail(IModItem item)
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Thumbnail", "Image Files:*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tif");
        if (await dialog.ShowDialog() is string result) {
            item.ThumbnailUri = result;
        }
    }

    [RelayCommand]
    private async Task Create()
    {
        string tmpOutput = Path.Combine(Path.GetTempPath(), "tkmm", Mod.Id.ToString());
        await Create(tmpOutput, cleanOutput: false);
        Directory.Delete(tmpOutput, recursive: true);
    }


    [RelayCommand]
    private async Task CreateAndInstall()
    {
        string output = ProfileManager.GetModFolder(Mod.Id);
        await Create(output, cleanOutput: true);
        
        if (FolderModReader.FromInternal(output) is Mod copy) {
            ProfileManager.Shared.Mods.TryInsert(copy);
            ProfileManager.Shared.Current.Mods.TryInsert(copy);
            ProfileManager.Shared.Current.Selected = copy;
        }
    }

    private async Task<(bool, string?)> Create(string output, bool cleanOutput)
    {
        try {
            if (string.IsNullOrEmpty(SourceFolder)) {
                App.Toast("Packaging requires a mod to package. Please provide a mod folder.");
                return (false, default);
            }

            if (string.IsNullOrEmpty(Mod.Name)) {
                Mod.Name = Path.GetFileName(SourceFolder);
            }

            if (string.IsNullOrEmpty(ExportPath)) {
                ExportPath = Path.Combine(Path.GetDirectoryName(SourceFolder) ?? string.Empty,
                                          $"{Path.GetFileName(SourceFolder)}.tkcl");
            }

            if (!string.IsNullOrEmpty(Mod.ThumbnailUri)) {
                string relativeThumbnailUri = Path.Combine(SourceFolder, Mod.ThumbnailUri);
                if (File.Exists(relativeThumbnailUri)) {
                    Mod.ThumbnailUri = relativeThumbnailUri;
                }
            }

            if (cleanOutput && Directory.Exists(output)) {
                Directory.Delete(output, recursive: true);
            }

            await Task.Run(async () => {
                PackageBuilder.CreateMetaData(Mod, output);
                await PackageBuilder.CopyContents(Mod, SourceFolder, output);
                PackageBuilder.Package(output, ExportPath);
            });

            return (true, output);
        } catch (Exception exc) {
            App.ToastError(exc);
            AppStatus.Set(exc.Message, isWorkingStatus: false, temporaryStatusTime: 1.5, logLevel: LogLevel.None);

            return (false, default);
        }
    }

    [RelayCommand]
    private async Task ImportInfo()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Import Mod Info", "Mods:info.json;*.tkcl|JSON Metadata:info.json|Mod Archive:*.tkcl");
        if (await dialog.ShowDialog() is string result) {
            Stream? stream;
            if (result.EndsWith("info.json")) {
                stream = File.OpenRead(result);
            }
            else {
                ZipArchive archive = ZipFile.OpenRead(result);
                stream = archive.Entries.FirstOrDefault(x => x.Name == "info.json")?.Open();
            }

            if (stream is null) {
                AppStatus.Set("Could not read mod metadata!", "fa-solid fa-triangle-exclamation", temporaryStatusTime: 1.5, isWorkingStatus: false);
                return;
            }

            Mod = JsonSerializer.Deserialize<Mod>(stream)
                ?? throw new InvalidOperationException("""
                    Error parsing metadata: The JSON deserializer returned null.
                    """);

            await stream.DisposeAsync();
        }
    }

    [RelayCommand]
    private async Task ExportMetadata()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Export Mod Metadata");
        if (await dialog.ShowDialog() is string result) {
            PackageBuilder.CreateMetaData(Mod, result);
            AppStatus.Set("Exported metadata!", "fa-solid fa-circle-check", temporaryStatusTime: 1.5, isWorkingStatus: false);
        }
    }

    [RelayCommand]
    private Task WriteMetadata()
    {
        if (!string.IsNullOrEmpty(SourceFolder) && Directory.Exists(SourceFolder)) {
            PackageBuilder.CreateMetaData(Mod, SourceFolder, useSourceFolderName: true);
            AppStatus.Set("Exported metadata!", "fa-solid fa-circle-check", temporaryStatusTime: 1.5, isWorkingStatus: false);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task Refresh()
    {
        string store = SourceFolder;
        SourceFolder = string.Empty;
        SourceFolder = store;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ImportOptionGroup()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Import Mod Option Group");
        if (await dialog.ShowDialog() is string result) {
            string name = Path.GetFileName(result);
            string output = Path.Combine(SourceFolder, PackageBuilder.OPTIONS, name);
            if (Directory.Exists(output)) {
                ContentDialog warningDialog = new() {
                    Title = "Warning",
                    Content = $"The option group '{name}' already exists, would you like to replace it?",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                };

                if (await warningDialog.ShowAsync() is not ContentDialogResult.Primary) {
                    return;
                }

                Directory.Delete(output, recursive: true);

                if (Mod.OptionGroups.FirstOrDefault(x => x.SourceFolder == output) is ModOptionGroup target) {
                    Mod.OptionGroups.Remove(target);
                }
            }

            DirectoryOperations.CopyDirectory(result, output, overwrite: true);

            if (ModOptionGroup.FromFolder(output) is ModOptionGroup group) {
                Mod.OptionGroups.Add(group);
            }
        }
    }

    [RelayCommand]
    private static async Task ImportOption(ModOptionGroup group)
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Import Mod Option");
        if (await dialog.ShowDialog() is string result) {
            string name = Path.GetFileName(result);
            string output = Path.Combine(group.SourceFolder, name);
            if (Directory.Exists(output)) {
                ContentDialog warningDialog = new() {
                    Title = "Warning",
                    Content = $"The option '{name}' already exists in the group '{group.Name}', would you like to replace it?",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                };

                if (await warningDialog.ShowAsync() is not ContentDialogResult.Primary) {
                    return;
                }

                Directory.Delete(output, recursive: true);

                if (group.Options.FirstOrDefault(x => x.SourceFolder == output) is ModOption target) {
                    group.Options.Remove(target);
                }
            }

            DirectoryOperations.CopyDirectory(result, output);
            if (ModOption.FromFolder(output) is ModOption option) {
                group.Options.Add(option);
            }
        }
    }

    [RelayCommand]
    private async Task RemoveOptionGroup(ModOptionGroup target)
    {
        if (await WarnRemove(target) == false) {
            return;
        }

        if (Mod.OptionGroups.Remove(target)) {
            Directory.Delete(target.SourceFolder, true);
        }
    }

    [RelayCommand]
    private async Task RemoveOption(ModOption target)
    {
        if (Mod.OptionGroups.FirstOrDefault(x => x.Options.Contains(target)) is not ModOptionGroup group) {
            return;
        }

        if (await WarnRemove(target) == false) {
            return;
        }

        if (group.Options.Remove(target)) {
            Directory.Delete(target.SourceFolder, true);
        }
    }

    private static async Task<bool> WarnRemove(IModItem target)
    {
        ContentDialog dialog = new() {
            Title = "Warning",
            Content = $"""
            This action will delete the source folder in '{target.SourceFolder}' and cannot be undone.

            Are you sure you would like to delete '{target.Name}'?
            """,
            PrimaryButtonText = "Delete Permanently",
            SecondaryButtonText = "Cancel"
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    partial void OnSourceFolderChanged(string value)
    {
        string metadataPath = Path.Combine(value, PackageBuilder.METADATA);
        if (File.Exists(metadataPath)) {
            using FileStream fs = File.OpenRead(metadataPath);
            Mod = JsonSerializer.Deserialize<Mod>(fs) ?? Mod;
        }
        else {
            Mod.Id = Guid.NewGuid();
        }

        Mod.RefreshOptions(value);
    }
}