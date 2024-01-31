using ConfigFactory;
using ConfigFactory.Avalonia;
using ConfigFactory.Models;
using FluentAvalonia.UI.Windowing;
using Tkmm.Core;
using Tkmm.Helpers;
using Tkmm.Models;

namespace Tkmm.Views;

public partial class ShellView : AppWindow
{
    private static readonly ConfigPage _configPage = new();

    public ShellView()
    {
        InitializeComponent();
        SplashScreen = new SplashScreen();

        if (_configPage.DataContext is ConfigPageModel model) {
            model.SecondaryButtonIsEnabled = false;
            model.Append<Config>();
            model.Append<TotkConfig>();
        }

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        PageManager.Shared.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(PageManager.Current)) {
                MainNavigation.Content = PageManager.Shared.Current?.Content;
            }
        };
    }
}
