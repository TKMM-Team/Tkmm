using Avalonia.Controls;
using Tkmm.Core;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class TkOptimizerPageView : UserControl
{
    public TkOptimizerPageView()
    {
        InitializeComponent();
        DataContext = new TkOptimizerPageViewModel();
        
        // Ensure the startup
        // config is written
        TKMM.MergeBasic();
    }

    public static void OnPageFocused(TkOptimizerPageView? view)
    {
        if (view?.DataContext is TkOptimizerPageViewModel vm) {
            vm.Reload();
        }
    }
} 