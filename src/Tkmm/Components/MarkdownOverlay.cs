using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvaMark;
using Tkmm.Views.Common;

namespace Tkmm.Components;

public static class MarkdownOverlay
{
    public static async Task ShowAsync(string markdown)
    {
        var closed = new TaskCompletionSource();

        Button close = new() {
            Content = Locale["Action_Close"],
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0),
            MinWidth = 100
        };

        OverlayCard card = new() {
            CardMaxWidth = 720,
            CardMinWidth = 420,
            CardMargin = new Thickness(40, 20),
            CardPadding = new Thickness(20),
            Content = new DockPanel {
                LastChildFill = true,
                Children = {
                    close,
                    new ScrollViewer {
                        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                        Content = new MarkdownViewer {
                            Markdown = markdown,
                            FontSize = 14
                        }
                    }
                }
            }
        };

        DockPanel.SetDock(close, Avalonia.Controls.Dock.Bottom);

        OverlayModal modal = new(card);
        close.Click += (_, _) => _ = Close();
        modal.Show();
        await closed.Task;
        return;

        async Task Close()
        {
            await modal.HideAsync();
            closed.TrySetResult();
        }
    }
}
