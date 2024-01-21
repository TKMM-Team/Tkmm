using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Components;
using Tkmm.Core.Models.Mods;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using Avalonia.Controls;

namespace Tkmm.ViewModels.Pages;

public partial class ToolsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _exportFile = string.Empty;

    [ObservableProperty]
    private Mod _mod = new();

    public ICommand BrowseExportFileCommand { get; }
    public ICommand BrowseSourceFolderCommand { get; }
    public ICommand BrowseThumbnailCommand { get; }

    private Window _window;

    public ToolsPageViewModel(Window window)
    {
        _window = window;

        BrowseExportFileCommand = new RelayCommand(BrowseExportFile);
        BrowseSourceFolderCommand = new RelayCommand(BrowseSourceFolder);
        BrowseThumbnailCommand = new RelayCommand(BrowseThumbnail);
    }

    private async void BrowseExportFile()
    {
        var dialog = new SaveFileDialog
        {
            DefaultExtension = "tkcl",
            Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "TKCL Files", Extensions = new List<string> { "tkcl" } } }
        };
        var result = await dialog.ShowAsync(_window);
        if (result != null)
        {
            ExportFile = result;
        }
    }

    private async void BrowseSourceFolder()
    {
        var dialog = new OpenFolderDialog();
        var result = await dialog.ShowAsync(_window);
        if (result != null)
        {
            Mod.SourceFolder = result;
        }
    }

    private async void BrowseThumbnail()
    {
        var dialog = new OpenFileDialog();
        dialog.Filters.Add(new FileDialogFilter { Name = "Image files", Extensions = new List<string> { "png", "jpg", "" } });
        var result = await dialog.ShowAsync(_window);
        if (result != null && result.Length > 0)
        {
            Mod.ThumbnailUri = result[0];
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