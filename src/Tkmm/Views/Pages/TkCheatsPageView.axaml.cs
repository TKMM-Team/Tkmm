using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Tkmm.ViewModels.Pages;
using TkSharp.Core;

namespace Tkmm.Views.Pages
{
    public partial class TkCheatsPageView : UserControl
    {
        public TkCheatsPageView()
        {
            InitializeComponent();
            DataContext = new TkCheatsPageViewModel();
        }

        public static void OnPageFocused(TkCheatsPageView? view)
        {
            TkLog.Instance.LogInformation("Cheats page activated; refreshing version.");
            
            if (view?.DataContext is TkCheatsPageViewModel vm) {
                vm.RefreshVersion();
            }
        }
    }
}