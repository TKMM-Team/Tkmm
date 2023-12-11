using ConfigFactory;
using ConfigFactory.Avalonia;
using ConfigFactory.Models;
using FluentAvalonia.UI.Windowing;
using Tcml.Core;
using Tcml.Helpers;
using Tcml.Helpers.Models;

namespace Tcml.Views;

public partial class ShellView : AppWindow
{
    private static readonly ConfigPage _configPage = new();

    public ShellView()
    {
        InitializeComponent();

        if (_configPage.DataContext is ConfigPageModel model) {
            model.SecondaryButtonIsEnabled = false;
            model.Append<Config>();
            model.Append<TotkConfig>();
        }

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        MainNavigation.MenuItemsSource = PageManager.Shared.Pages;
        MainNavigation.SelectionChanged += (s, e) => {
            if (e.IsSettingsSelected) {
                MainNavigation.Content = _configPage;
            }
            else if (e.SelectedItem is PageModel page) {
                MainNavigation.Content = page.Content;
            }
        };
    }
}
