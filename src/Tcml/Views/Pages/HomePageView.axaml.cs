using Avalonia.Controls;
using Tcml.ViewModels.Pages;

namespace Tcml.Views.Pages;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
        DataContext = new HomePageViewModel();
    }
}
