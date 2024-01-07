using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;
public partial class ModsPageView : UserControl
{
    public ModsPageView()
    {
        InitializeComponent();
        DataContext = new ModsPageViewModel();
    }
}
