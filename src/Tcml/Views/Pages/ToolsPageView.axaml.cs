using Avalonia.Controls;
using Tcml.ViewModels.Pages;

namespace Tcml.Views.Pages;

public partial class ToolsPageView : UserControl
{
    public ToolsPageView()
    {
        InitializeComponent();
        DataContext = new ToolsPageViewModel();
    }
}
