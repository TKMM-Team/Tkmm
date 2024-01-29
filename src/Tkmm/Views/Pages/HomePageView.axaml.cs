using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Tkmm.Core.Models.Mods;
using Tkmm.Helpers;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
        DataContext = new HomePageViewModel();

        DropTarget.AddHandler(DragDrop.DragEnterEvent, DragEnterEvent);
        DropTarget.AddHandler(DragDrop.DragLeaveEvent, DragLeaveEvent);
        DropTarget.AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

    public async void DragDropEvent(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (var path in paths.Select(x => x.Path.LocalPath)) {
                if (DataContext is HomePageViewModel homePage) {
                    if (await ModHelper.Import(path) is Mod mod) {
                        homePage.CurrentMod = mod;
                    }
                }
            }
        }
        else if (e.Data.GetText() is string arg && DataContext is HomePageViewModel homePage) {
            if (await ModHelper.Import(arg) is Mod mod) {
                homePage.CurrentMod = mod;
            }
        }

        DragFadeMask.IsVisible = false;
    }

    public void DragEnterEvent(object? sender, DragEventArgs e)
    {
        DragFadeMask.IsVisible = true;
    }

    public void DragLeaveEvent(object? sender, DragEventArgs e)
    {
        DragFadeMask.IsVisible = false;
    }
}
