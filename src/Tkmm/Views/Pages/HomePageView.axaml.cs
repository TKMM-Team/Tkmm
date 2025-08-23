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

        var listBox = this.FindControl<ListBox>("ModsList");
        if (listBox is not null) {
            listBox.PropertyChanged += (_, args) => {
                if (args.Property.Name == nameof(ListBox.SelectedItem) && listBox.SelectedItem is not null) {
                    listBox.ScrollIntoView(listBox.SelectedItem);
                }
            };
        }
    }

    private void GridSplitter_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
    {
        HomePageViewModel.Layout.TopPanel = ResizeGridTarget.RowDefinitions[0].Height;
        HomePageViewModel.Layout.LowerPanel = ResizeGridTarget.RowDefinitions[2].Height;
        HomePageViewModel.Layout.Save();
    }
}
