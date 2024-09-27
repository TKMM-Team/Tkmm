using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using System.Timers;
using Avalonia;
using Tkmm.Controls.Keyboard;
using Tkmm.Controls.Keyboard.Layout;
using Tkmm.Helpers;
using Tkmm.Models;

namespace Tkmm.Views;

public partial class ShellView : AppWindow
{
    private bool _iskeyboardCooldown;
    private bool _iskeyboardCooldownStarted;

    private readonly System.Timers.Timer _keyboardCoolDown;

    public ShellView()
    {
        VirtualKeyboard.AddLayout<VirtualKeyboardLayoutUS>();
        VirtualKeyboard.SetDefaultLayout(() => typeof(VirtualKeyboardLayoutUS));

        InitializeComponent();

        _keyboardCoolDown = new System.Timers.Timer();
        _keyboardCoolDown.Interval = 200;
        _keyboardCoolDown.Elapsed += ResetCoolDown;

        AddHandler(GotFocusEvent, OpenVirtualKeyboard);

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

    private void ResetCoolDown(object? sender, ElapsedEventArgs e)
    {
        _iskeyboardCooldown = false;
        _iskeyboardCooldownStarted = false;
        _keyboardCoolDown.Stop();
    }

    private void OpenVirtualKeyboard(object? sender, GotFocusEventArgs e)
    {
        if (_iskeyboardCooldown && !_iskeyboardCooldownStarted) {
            _iskeyboardCooldownStarted = true;
            _keyboardCoolDown.Start();
        }

        if (e.Source?.GetType() != typeof(TextBox) || Keyboard.IsVisible) {
            return;
        }

        var tb = e.Source as Control;

        switch (tb?.Tag?.ToString()) {
            case "numpad":
                Keyboard.ShowKeyboard((TextBox)e.Source, typeof(VirtualKeyboardLayoutNumpad));
                break;
            default:
                Keyboard.ShowKeyboard((TextBox)e.Source);
                break;
        }
        
        _iskeyboardCooldown = true;
    }
}