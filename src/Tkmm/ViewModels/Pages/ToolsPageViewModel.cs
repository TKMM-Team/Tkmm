using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using System.IO.Compression;
using System.Text.Json;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Models.Mods;

namespace Tkmm.ViewModels.Pages;

public partial class ToolsPageViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions _jsonOptions = new() {
        WriteIndented = true
    };

    [ObservableProperty]
    private string _exportFile = string.Empty;

    [ObservableProperty]
    private Mod _mod = new();

    [RelayCommand]
    private async Task BrowseExportFile()
    {
        BrowserDialog dialog = new(BrowserMode.SaveFile, "Export Location", "TKCL Files:*.tkcl");
        if (await dialog.ShowDialog() is string result) {
            ExportFile = result;
        }
    }

    [RelayCommand]
    private async Task BrowseSourceFolder()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFolder, "Source Folder");
        if (await dialog.ShowDialog() is string result) {
            Mod.SourceFolder = result;
        }
    }

    [RelayCommand]
    private async Task BrowseThumbnail()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Thumbnail", "Image Files:*.bmp;*.gif;*.jpg;*.jpeg;*.tif");
        if (await dialog.ShowDialog() is string result) {
            Mod.ThumbnailUri = result;
        }
    }

    [RelayCommand]
    private async Task Create()
    {
        AppStatus.Set("Package building...");
        PackageGenerator packageGenerator = new(Mod);
        await packageGenerator.Build();
        packageGenerator.Save(ExportFile, clearOutputFolder: true);

        AppStatus.Set("Package built");
    }

    [RelayCommand]
    private async Task ImportInfo()
    {
        BrowserDialog dialog = new(BrowserMode.OpenFile, "Import Mod Info", "JSON Metadata:info.json|Mod Archive:*.tkcl");
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
    private async Task ExportInfo()
    {
        BrowserDialog dialog = new(BrowserMode.SaveFile, "Export Mod Info", "JSON Metadata:info.json", "info.json");
        if (await dialog.ShowDialog() is string result) {
            using FileStream fs = File.Create(result);
            JsonSerializer.Serialize(fs, Mod, _jsonOptions);

            AppStatus.Set("Exported mod metadata!", "fa-solid fa-circle-check", temporaryStatusTime: 1.5, isWorkingStatus: false);
        }
    }
}