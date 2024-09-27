using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using System.Timers;
using Tkmm.Controls.Keyboard;
using Tkmm.Controls.Keyboard.Layout;
using Tkmm.Helpers;
using Tkmm.Models;

namespace Tkmm.Views;

public partial class ShellView : AppWindow
{

    private VirtualKeyboardTextInputMethod virtualKeyboardTextInput = null;
    private bool iskeyboardCooldown;
    private bool iskeyboardCooldownStarted;

    private Tkmm.Controls.Keyboard.VirtualKeyboard keyboard;

    private System.Timers.Timer? keyboardCoolDown;

    public ShellView()
    {
        Tkmm.Controls.Keyboard.VirtualKeyboard.AddLayout<VirtualKeyboardLayoutUS>();
        Tkmm.Controls.Keyboard.VirtualKeyboard.SetDefaultLayout(() => typeof(VirtualKeyboardLayoutUS));

        InitializeComponent();

        virtualKeyboardTextInput = new VirtualKeyboardTextInputMethod((AppWindow)this);

        keyboard = this.GetControl<Tkmm.Controls.Keyboard.VirtualKeyboard>("VirtualKeyboardControl");

        keyboardCoolDown = new System.Timers.Timer();
        keyboardCoolDown.Interval = 200;
        keyboardCoolDown.Elapsed += resetCoolDown;

        this.AddHandler<GotFocusEventArgs>(Control.GotFocusEvent, openVirtualKeyboard);

        SplashScreen = new SplashScreen();

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        Bitmap bitmap = new(AssetLoader.Open(new Uri("avares://Tkmm/Assets/icon.ico")));
        Icon = bitmap.CreateScaledBitmap(new(48, 48));

        PageManager.Shared.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(PageManager.Current)) {
                MainNavigation.Content = PageManager.Shared.Current?.Content;
            }
        };
    }
    private void resetCoolDown(object? sender, ElapsedEventArgs e)
    {


        iskeyboardCooldown = false;
        iskeyboardCooldownStarted = false;
        keyboardCoolDown.Stop();
    }
    private void openVirtualKeyboard(object? sender, GotFocusEventArgs e)
    {
        if (iskeyboardCooldown && !iskeyboardCooldownStarted)
        {
            iskeyboardCooldownStarted = true;
            keyboardCoolDown.Start();
        }

        if (e.Source.GetType() == typeof(TextBox) && !keyboard.IsVisible)
        {

            //virtualKeyboardTextInput.SetActive(true, e);
            var tb = e.Source as Control;




            switch (tb.Tag?.ToString())
            {
                case "numpad":
                    keyboard.ShowKeyboard(e.Source as TextBox, typeof(VirtualKeyboardLayoutNumpad));
                    break;
                default:
                    keyboard.ShowKeyboard(e.Source as TextBox);
                    break;
            }


            //keyboard.IsVisible = true;
            //((Control)keyboard.Parent).IsVisible = true;

            iskeyboardCooldown = true;
        }
    }
}
