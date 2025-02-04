using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class TotKOptimizerPageView : UserControl
{
    public TotKOptimizerPageView()
    {
        InitializeComponent();
        DataContext = new TotKOptimizerPageViewModel();
    }
} 