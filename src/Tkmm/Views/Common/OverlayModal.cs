using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

namespace Tkmm.Views.Common;

public sealed class OverlayModal(Control content) : IDisposable
{
    private readonly DialogHost _host = new() {
        Content = content
    };
    private OverlayLayer? _overlayLayer;
    private bool _isShown;

    public void Show()
    {
        if (_isShown) {
            return;
        }

        if (OverlayLayer.GetOverlayLayer(App.XamlRoot) is not { } overlayLayer) {
            return;
        }

        _overlayLayer = overlayLayer;
        if (!overlayLayer.Children.Contains(_host)) {
            overlayLayer.Children.Add(_host);
        }

        _isShown = true;
    }

    public void Hide()
    {
        if (!_isShown) {
            return;
        }

        _overlayLayer?.Children.Remove(_host);
        _overlayLayer = null;
        _isShown = false;
    }

    public void Dispose() => Hide();

    public static void Show(Control content, CancellationToken cancellationToken = default)
    {
        OverlayModal modal = new(content);
        modal.Show();

        if (cancellationToken.CanBeCanceled) {
            _ = Task.Run(async () => {
                try {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException) {
                }

                await Dispatcher.UIThread.InvokeAsync(modal.Hide);
            }, CancellationToken.None);
        }
    }
}