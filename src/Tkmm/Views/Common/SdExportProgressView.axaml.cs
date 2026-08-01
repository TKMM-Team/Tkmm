using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FluentAvalonia.UI.Controls;

namespace Tkmm.Views.Common;

public partial class SdExportProgressView : UserControl
{
    private readonly DialogHost _host;
    private OverlayLayer? _overlayLayer;

    public SdExportProgressView()
    {
        InitializeComponent();
        _host = new DialogHost {
            Content = this
        };
    }

    public string Title
    {
        get => TitleText.Text ?? string.Empty;
        set => TitleText.Text = value;
    }

    public void Show()
    {
        if (OverlayLayer.GetOverlayLayer(App.XamlRoot) is not { } overlayLayer) {
            return;
        }

        _overlayLayer = overlayLayer;
        if (!overlayLayer.Children.Contains(_host)) {
            overlayLayer.Children.Add(_host);
        }
    }

    public void Hide()
    {
        _overlayLayer?.Children.Remove(_host);
        _overlayLayer = null;
    }

    public void SetIndeterminate(string status)
    {
        StatusText.Text = status;
        PercentText.Text = string.Empty;
        DetailText.Text = string.Empty;
        DetailText.IsVisible = false;
        Progress.IsIndeterminate = true;
        Progress.Value = 0;
    }

    public void BeginCopy(string status)
    {
        StatusText.Text = status;
        PercentText.Text = "0%";
        DetailText.Text = string.Empty;
        DetailText.IsVisible = false;
        Progress.IsIndeterminate = false;
        Progress.Value = 0;
    }

    public void SetProgress(string status, int copied, int total)
    {
        var percent = total <= 0 ? 0 : (int)(copied * 100.0 / total);

        StatusText.Text = status;
        PercentText.Text = $"{percent}%";
        if (total <= 0) {
            DetailText.Text = string.Empty;
            DetailText.IsVisible = false;
        }
        else {
            DetailText.Text = string.Format(Locale["MergeActions_ExportProgressFiles"], copied, total);
            DetailText.IsVisible = true;
        }

        Progress.IsIndeterminate = false;
        Progress.Value = percent;
    }
}
