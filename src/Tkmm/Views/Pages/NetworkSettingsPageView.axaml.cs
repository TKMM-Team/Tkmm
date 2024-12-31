using Avalonia.Controls;
using Avalonia.Input;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;
public partial class NetworkSettingsPageView : UserControl
{
    public NetworkSettingsPageView()
    {
        InitializeComponent();
        DataContext = new NetworkSettingsPageViewModel();
    }

    private async void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is NetworkSettingsPageViewModel vm) {
            await vm.ConnectToNetworkAsync();
        }
    }
}