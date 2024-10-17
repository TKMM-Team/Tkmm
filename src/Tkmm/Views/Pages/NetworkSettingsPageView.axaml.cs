using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class NetworkSettingsPageView : UserControl
{
    public NetworkSettingsPageView()
    {
        InitializeComponent();
        DataContext = new NetworkSettingsPageViewModel();
    }
}