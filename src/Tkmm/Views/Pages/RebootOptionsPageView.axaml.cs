using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class RebootOptionsPageView : UserControl
{
    public RebootOptionsPageView()
    {
        InitializeComponent();
        DataContext = new RebootOptionsPageViewModel();
    }
}