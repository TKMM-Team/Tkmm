using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using FluentAvalonia.UI.Windowing;
using Tkmm.Launcher.ViewModels;

namespace Tkmm.Launcher.Views;

public partial class ShellView : AppWindow
{
    private static readonly (string, string)[] _images = [
        ("Background-A.jpg", "#6adfa6"),
        ("Background-B.jpg", "#fdfdfd"),
        ("Background-C.jpg", "#58b98a"),
        ("Background-D.png", "#ff0400"),
        ("Background-E.jpg", "#bb9b45"),
        ("Background-F.jpg", "#a21e16"),
        ("Background-G.png", "#c71916"),
    ];

    private static readonly List<(Bitmap Image, IBrush Color)> _backgrounds = [];

    private readonly Timer _timer;

    static ShellView()
    {
        foreach ((var image, var color) in _images) {
            using Stream stream = AssetLoader.Open(new($"avares://Tkmm.Launcher/Assets/{image}"));
            Bitmap bmp = new(stream);
            _backgrounds.Add((bmp, Brush.Parse(color)));
        }
    }

    public ShellView()
    {
        InitializeComponent();
        DataContext = new ShellViewModel(this);
        Client.PointerPressed += (s, e) => BeginMoveDrag(e);

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.Height = 0;

        Background.Source = _backgrounds[^1].Image;
        StaticBackground.Source = _backgrounds[0].Image;

        _timer = new(async (e) => {
            Dispatcher.UIThread.Invoke(() => {
                for (int i = 0; i < _backgrounds.Count; i++) {
                    if (Background.Source == _backgrounds[i].Image) {
                        (var image, var color) = _backgrounds[++i >= _backgrounds.Count ? 0 : i];
                        Background.Source = image;
                        IconBack.Fill = color;
                        IconFront.Fill = color;
                    }
                }
            });

            await Task.Delay(TimeSpan.FromSeconds(10));

            Dispatcher.UIThread.Invoke(() => {
                for (int i = 0; i < _backgrounds.Count; i++) {
                    if (Background.Source == _backgrounds[i].Image) {
                        StaticBackground.Source = _backgrounds[++i >= _backgrounds.Count ? 0 : i].Image;
                    }
                }
            });

        });

        _timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(20));
    }
}