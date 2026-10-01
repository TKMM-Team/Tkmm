using Avalonia.Input;
using Avalonia.Interactivity;
using Tkmm.ViewModels.Pages;
using Tkmm.Views.Common;

namespace Tkmm.Views.Pages;

public partial class GameBananaImageViewerOverlay : OverlayCard
{
    private static OverlayModal? _modal;

    private GameBananaImageViewerOverlay()
    {
        InitializeComponent();
    }

    public static void Show(GameBananaModPageViewModel viewModel)
    {
        if (_modal is not null) {
            return;
        }

        var bounds = App.XamlRoot.Bounds;
        GameBananaImageViewerOverlay overlay = new() {
            DataContext = viewModel,
            CardMinWidth = bounds.Width * 0.9,
            CardMaxWidth = bounds.Width * 0.9,
            CardMinHeight = bounds.Height * 0.9,
            CardMaxHeight = bounds.Height * 0.9
        };

        _modal = new OverlayModal(overlay);
        _modal.Show();
        overlay.Focus();
    }

    public static async Task CloseAsync()
    {
        if (_modal is null) {
            return;
        }

        var modal = _modal;
        _modal = null;
        await modal.HideAsync();
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e) => _ = CloseAsync();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is not GameBananaModPageViewModel viewModel) {
            return;
        }
        
        switch (e.Key) {
            case Key.Left:
                viewModel.PreviousImageCommand.Execute(null);
                e.Handled = true;
                return;
            case Key.Right:
                viewModel.NextImageCommand.Execute(null);
                e.Handled = true;
                return;
            case Key.Escape:
                e.Handled = true;
                _ = CloseAsync();
                return;
        }
    }
}
