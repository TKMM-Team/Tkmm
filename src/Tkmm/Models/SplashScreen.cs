using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using Tkmm.Core.Components;
using Tkmm.Helpers;

namespace Tkmm.Models;

public class SplashScreen : IApplicationSplashScreen
{
    private static readonly Bitmap _image;

    static SplashScreen()
    {
        using Stream stream = AssetLoader.Open(new("avares://Tkmm/Assets/SplashScreen.jpg"));
        _image = new Bitmap(stream);
    }

    public string? AppName { get; }
    public IImage? AppIcon { get; }
    public object SplashScreenContent { get; } = new Image {
        Source = _image,
        Stretch = Stretch.UniformToFill
    };

    public int MinimumShowTime { get; } = 1500;

    public async Task RunTasks(CancellationToken cancellationToken)
    {
        List<Task> tasks = [];

        foreach (var mod in ModManager.Shared.Mods) {
            tasks.Add(ModHelper.ResolveThumbnail(mod));
        }

        await Task.WhenAll(tasks);
    }
}
