using Avalonia.Controls;
using Avalonia.VisualTree;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages
{
    public partial class CheatsPageView : UserControl
    {
        public CheatsPageView()
        {
            InitializeComponent();
            DataContext = new CheatsPageViewModel();
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (DataContext is CheatsPageViewModel vm) {
                vm.RefreshVersion();
            }
        }
    }
} 