using Avalonia.Controls;
using Avalonia.Input;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;
public partial class GameBananaPageView : UserControl
{
    public GameBananaPageView()
    {
        InitializeComponent();
        DataContext = new GameBananaPageViewModel();
        
        Focusable = true;
        KeyDown += OnKeyDown;
    }

    private async void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is GameBananaPageViewModel vm) {
            await vm.Browser.Search(ModsViewer);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not GameBananaPageViewModel vm || vm.Viewer?.SelectedMod is null) {
            return;
        }
        
        if (e.Key == Key.Left && vm.Viewer.SelectedImageIndex > 0) {
            vm.Viewer.SelectedImageIndex--;
            e.Handled = true;
        }
        else if (e.Key == Key.Right && vm.Viewer.SelectedImageIndex < vm.Viewer.Images.Count - 1) {
            vm.Viewer.SelectedImageIndex++;
            e.Handled = true;
        }
    }
}