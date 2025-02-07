using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class TkOptimizerPageView : UserControl
{
    public TkOptimizerPageView()
    {
        InitializeComponent();
        DataContext = new TkOptimizerPageViewModel();
    }

    public static void OnPageFocused(TkOptimizerPageView? view)
    {
        if (view?.DataContext is TkOptimizerPageViewModel vm) {
            vm.Reload();
        }
    }
} 