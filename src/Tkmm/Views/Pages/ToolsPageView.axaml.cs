using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class ToolsPageView : UserControl
{
    public ToolsPageView()
    {
        InitializeComponent();
        DataContext = new ToolsPageViewModel();
    }
}
