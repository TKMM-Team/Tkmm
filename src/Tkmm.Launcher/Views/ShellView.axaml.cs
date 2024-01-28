using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Tkmm.Launcher.Views;
public partial class ShellView : Window
{
    private readonly Timer _timer;

    private static readonly Bitmap _backgroundA;
    private static readonly Bitmap _backgroundB;
    private static readonly Bitmap _backgroundC;

    static ShellView()
    {
        using (Stream stream = AssetLoader.Open(new("avares://Tkmm.Launcher/Assets/Background-A.jpg"))) {
            _backgroundA = new Bitmap(stream);
        }

        using (Stream stream = AssetLoader.Open(new("avares://Tkmm.Launcher/Assets/Background-B.jpg"))) {
            _backgroundB = new Bitmap(stream);
        }

        using (Stream stream = AssetLoader.Open(new("avares://Tkmm.Launcher/Assets/Background-C.jpg"))) {
            _backgroundC = new Bitmap(stream);
        }
    }

    public ShellView()
    {
        InitializeComponent();
        Client.PointerPressed += (s, e) => BeginMoveDrag(e);

        Background.Source = _backgroundA;

        _timer = new((e) => {
            Dispatcher.UIThread.Invoke(() => {
                if (Background.Source == _backgroundA) {
                    Background.Source = _backgroundB;
                }
                else if (Background.Source == _backgroundB) {
                    Background.Source = _backgroundC;
                }
                else if (Background.Source == _backgroundC) {
                    Background.Source = _backgroundA;
                }
            });
        });

        _timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(20));
    }
}