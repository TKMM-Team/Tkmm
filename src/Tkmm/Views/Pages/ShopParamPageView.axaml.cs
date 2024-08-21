using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Tkmm.Attributes;
using Tkmm.Helpers;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

[Page(Page.ShopParam, "ShopParam overflow ordering tools", Symbol.Sort, "ShopParam Overflow Editor")]
public partial class ShopParamPageView : UserControl
{
    public ShopParamPageView()
    {
        InitializeComponent();
        DataContext = new ShopParamPageViewModel(ZoomBorder);
    }
}