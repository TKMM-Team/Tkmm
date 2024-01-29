using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Tkmm.Launcher.Views;
public partial class ShellView : Window
{
    private static readonly (string, string)[] _images = [
        ("Background-A.jpg", "#58b98a"),
        ("Background-B.jpg", "#fdfdfd"),
        ("Background-C.jpg", "#58b98a"),
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
        Client.PointerPressed += (s, e) => BeginMoveDrag(e);

        Background.Source = _backgrounds[0].Image;
        StaticBackground.Source = _backgrounds[1].Image;

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