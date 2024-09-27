using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tkmm.Controls.Keyboard.Layout;

namespace Tkmm.Controls.Keyboard.Layout
{
    public partial class VirtualKeyboardLayoutUS : KeyboardLayout
    {
        public VirtualKeyboardLayoutUS()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string LayoutName => "en-US";
    }
}
