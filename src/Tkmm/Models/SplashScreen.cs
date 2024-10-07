using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using Tkmm.Core;
using Tkmm.Helpers;
using Tkmm.Views.Common;
using PageManager = Tkmm.Components.PageManager;

namespace Tkmm.Models;

public class SplashScreen : IApplicationSplashScreen
{
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public string? AppName { get; }

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public IImage? AppIcon { get; }

    public object SplashScreenContent { get; } = new SplashScreenView();

    public int MinimumShowTime => 1500;

    public async Task RunTasks(CancellationToken cancellationToken)
    {
        await Task.WhenAll([
            ..TKMM.ModManager.Mods.Select(tkMod => ModHelper.ResolveThumbnail(tkMod))
        ]);

        PageManager.Shared.Focus(PageManager.Shared.Default);
    }
}