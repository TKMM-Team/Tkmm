using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.VisualTree;

namespace Tkmm.Controls.Keyboard;

public class VirtualKeyboardTextInputMethod
{
    private bool _isOpen;
    private TextInputOptions? _textInputOptions;

    private Window root = null;

    public VirtualKeyboardTextInputMethod(Window root)
    {
        this.root = root;
    }
    public VirtualKeyboardTextInputMethod()
    {

    }

    public async Task SetActive(bool active, GotFocusEventArgs e)
    {
        if (active && !_isOpen)
        {

            _isOpen = true;
            var oskReturn = await VirtualKeyboard.ShowDialog(_textInputOptions, root);

            if (e.Source.GetType() == typeof(TextBox))
            {
                ((TextBox)e.Source).Text = oskReturn;

            }

            _isOpen = false;
            _textInputOptions = null;

            if (root != null)
            {
                root!.Focus();
                root.FocusManager.ClearFocus();
                var r = root.GetVisualChildren();
                var t = r.Last();

                ((Control)t).Focus();

            }
            else if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow.Focus();
                desktop.MainWindow.FocusManager.ClearFocus();
            }

            e.Handled = true;

        }
    }
}