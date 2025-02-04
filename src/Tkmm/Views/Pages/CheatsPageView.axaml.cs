using Avalonia.Controls;
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
    }
} 