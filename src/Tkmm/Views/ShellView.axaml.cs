using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.Logging;
using Tkmm.Components;
using Tkmm.Models;
using Tkmm.ViewModels;
using Tkmm.Wizard;
using TkSharp.Core;

namespace Tkmm.Views;

public partial class ShellView : AppWindow
{
    public ShellView()
    {
        InitializeComponent();
        SplashScreen = new SplashScreen();

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        Bitmap bitmap = new(AssetLoader.Open(new Uri("avares://Tkmm/Assets/icon.ico")));
        Icon = bitmap.CreateScaledBitmap(new PixelSize(48, 48));

        PageManager.Shared.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(PageManager.Current)) {
                MainNavigation.Content = PageManager.Shared.Current?.Content;
            }
        };
    }

    /// <summary>
    /// Start the setup wizard if this the first setup.
    /// </summary>
    public async void InitializeWizard()
    {
        try {
            if (!ShellViewModel.Shared.IsFirstTimeSetup) {
                return;
            }
        
            SetupWizard wizard = new StandardSetupWizard(WizardPresenter);
            await wizard.Start();

            ShellViewModel.Shared.IsFirstTimeSetup = false;
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Setup Wizard initialization failed");
        }
    }
}
