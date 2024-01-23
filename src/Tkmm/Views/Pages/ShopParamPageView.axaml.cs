using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class ShopParamPageView : UserControl
{
    public ShopParamPageView()
    {
        InitializeComponent();
        DataContext = new ShopParamPageViewModel(ZoomBorder);
    }
}