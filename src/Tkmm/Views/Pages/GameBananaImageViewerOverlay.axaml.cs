using Avalonia.Input;
using Avalonia.Interactivity;
using Tkmm.ViewModels.Pages;
using Tkmm.Views.Common;

namespace Tkmm.Views.Pages;

public partial class GameBananaImageViewerOverlay : OverlayCard
{
    private static OverlayModal? _modal;
    private static GameBananaModPageViewModel? _viewModel;

    public GameBananaImageViewerOverlay()
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

        _viewModel = viewModel;
        _modal = new OverlayModal(overlay);
        _modal.Show();
        App.XamlRoot.AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    public static async Task CloseAsync()
    {
        if (_modal is null) {
            return;
        }

        App.XamlRoot.RemoveHandler(KeyDownEvent, OnPreviewKeyDown);
        _viewModel = null;

        var modal = _modal;
        _modal = null;
        await modal.HideAsync();
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e) => _ = CloseAsync();

    private static void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (_viewModel is null) {
            return;
        }

        switch (e.Key) {
            case Key.Left:
                _viewModel.PreviousImageCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Right:
                _viewModel.NextImageCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Escape:
                e.Handled = true;
                _ = CloseAsync();
                break;
        }
    }
}
