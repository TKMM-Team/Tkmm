using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.Logging;
using Tkmm.Components;
using Tkmm.Models;
using Tkmm.ViewModels;
using Tkmm.Wizard;
using TkSharp.Core;
#if SWITCH
using R2CSharp.Lib.ViewModels;
#endif

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
        
#if SWITCH
        ShellViewModel.Shared.PropertyChanged += (_, e) => {
            if (e.PropertyName != nameof(ShellViewModel.Shared.IsVisibleR2CMenu)) {
                return;
            }
            MainNavigation.IsEnabled = !ShellViewModel.Shared.IsVisibleR2CMenu;
            MainMenu.IsEnabled = !ShellViewModel.Shared.IsVisibleR2CMenu;
            PowerOptionsMenu.IsEnabled = !ShellViewModel.Shared.IsVisibleR2CMenu;
        };
        
        LoadReboot2Config();
#endif
    }

#if  SWITCH
    private async void LoadReboot2Config()
    {
        try {
            var viewModel = new CarouselPageViewModel();
            await R2CMenu.SetViewModelAsync(viewModel);
        }
        catch (Exception e) {
            TkLog.Instance.LogError(e, "Reboot2Config menu initialization failed");
        }
    }
#endif
    
    /// <summary>
    /// Start the setup wizard if this the first setup.
    /// </summary>
    public async void InitializeWizard()
    {
        try {
            if (!ShellViewModel.Shared.IsFirstTimeSetup) {
                return;
            }
#if SWITCH
            SetupWizard wizard = new NxSetupWizard(WizardPresenter);
#else
            SetupWizard wizard = new StandardSetupWizard(WizardPresenter);
#endif
            await wizard.Start();

            ShellViewModel.Shared.IsFirstTimeSetup = false;
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Setup Wizard initialization failed");
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (e.Key is Key.PageUp) {
            if (PageManager.Shared.Current?.Id is Page page && (int)(page -= 1) > -1) {
                PageManager.Shared.Focus(page);
            }
        }
        
        if (e.Key is Key.PageDown) {
            if (PageManager.Shared.Current?.Id is not Page page) {
                return;
            }

            page += 1;

            if (Enum.IsDefined(page)) {
                PageManager.Shared.Focus(page);
            }
        }

#if SWITCH
        if (e.Key is not (Key.LWin or Key.RWin)) {
            return;
        }
        if (ShellViewModel.Shared.IsVisibleR2CMenu) {
            _ = ShellViewModel.Shared.HideR2CMenu();
        }
        else {
            ShellViewModel.Shared.ShowR2CMenu();
        }
#endif
    }
}
