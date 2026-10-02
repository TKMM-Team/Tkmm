using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;

namespace Tkmm.Views.Common;

public sealed class OverlayModal(Control content) : IDisposable
{
    private readonly DialogHost _host = new() {
        Content = content
    };

    private Panel? _hostPanel;
    private bool _isShown;

    public void Show()
    {
        if (_isShown) {
            return;
        }

        if (App.XamlRoot is not Window { Content: Panel panel }) {
            return;
        }

        _hostPanel = panel;
        if (!panel.Children.Contains(_host)) {
            panel.Children.Add(_host);
        }

        _isShown = true;
    }

    public async Task HideAsync()
    {
        if (!_isShown) {
            return;
        }

        if (content is OverlayCard card) {
            await card.PlayCloseAsync();
        }

        RemoveFromOverlay();
    }

    private void Hide()
    {
        if (!_isShown) {
            return;
        }

        RemoveFromOverlay();
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

                await Dispatcher.UIThread.InvokeAsync(modal.HideAsync);
            }, CancellationToken.None);
        }
    }

    private void RemoveFromOverlay()
    {
        _hostPanel?.Children.Remove(_host);
        _hostPanel = null;
        _isShown = false;
    }
}
