using Avalonia.Controls;
using Tkmm.ViewModels.Dialogs;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Dialogs;

public partial class ModOptionsView : UserControl
{
    public ModOptionsView(ToolsPageViewModel toolsPageViewModel)
    {
        InitializeComponent();
        DataContext = new ModOptionsViewModel(toolsPageViewModel);
    }
}
