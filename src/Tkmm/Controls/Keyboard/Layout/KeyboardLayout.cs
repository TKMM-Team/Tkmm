using Avalonia.Controls;

namespace Tkmm.Controls.Keyboard.Layout;

public abstract class KeyboardLayout : UserControl
{
    public abstract string LayoutName { get; }
}