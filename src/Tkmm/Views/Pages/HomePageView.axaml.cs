using Avalonia;
using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

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
        HomePageViewModel.Layout.TopPanel = ResizeGridTarget.RowDefinitions[1].Height;
        HomePageViewModel.Layout.LowerPanel = ResizeGridTarget.RowDefinitions[3].Height;
        HomePageViewModel.Layout.Save();
    }
}
