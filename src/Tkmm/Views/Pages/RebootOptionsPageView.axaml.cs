using Avalonia.Controls;

namespace Tkmm.Views.Pages;

public partial class RebootOptionsPageView : UserControl
{
    public RebootOptionsPageView()
    {
        InitializeComponent();
        DataContext = new ViewModels.Pages.RebootOptionsPageViewModel();
    }
}