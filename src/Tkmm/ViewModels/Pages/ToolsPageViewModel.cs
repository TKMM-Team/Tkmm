using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Components;
using Tkmm.Core.Models.Mods;

namespace Tkmm.ViewModels.Pages;

public partial class ToolsPageViewModel : ObservableObject
{
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
        PackageGenerator packageGenerator = new(Mod, ExportFile);
        await packageGenerator.Build();

        AppStatus.Set("Package built");
    }
}