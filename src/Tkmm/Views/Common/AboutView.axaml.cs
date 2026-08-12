using Avalonia.Interactivity;

namespace Tkmm.Views.Common;

public partial class AboutView : OverlayCard
{
    private OverlayModal? _modal;
    private TaskCompletionSource? _closed;

    public AboutView()
    {
        InitializeComponent();
    }

    public static async Task ShowAsync(string markdown)
    {
        AboutView view = new() {
            TitleText = { Text = Locale["Dialog_About"] },
            Markdown = { Markdown = markdown }
        };
        await view.ShowAsync();
    }

    private Task ShowAsync()
    {
        _closed = new TaskCompletionSource();
        _modal ??= new OverlayModal(this);
        _modal.Show();
        return _closed.Task;
    }

    private void Ok_OnClick(object? sender, RoutedEventArgs e)
    {
        _modal?.Hide();
        _closed?.TrySetResult();
    }
}
