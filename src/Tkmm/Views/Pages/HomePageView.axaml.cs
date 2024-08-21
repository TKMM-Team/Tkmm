using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Tkmm.Attributes;
using Tkmm.Helpers;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

[Page(Page.Home, nameof(Page.Home), Symbol.Home, isDefault: true)]
public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
        DataContext = new HomePageViewModel();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Re-initialize to set behaviors 
        InitializeComponent();

        base.OnAttachedToVisualTree(e);
    }

    private void GridSplitter_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
    {
        HomePageViewModel.Layout.TopPanel = ResizeGridTarget.RowDefinitions[0].Height;
        HomePageViewModel.Layout.LowerPanel = ResizeGridTarget.RowDefinitions[2].Height;
        HomePageViewModel.Layout.Save();
    }
}
