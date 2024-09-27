using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Tkmm.Controls.Keyboard.Layout
{
    public partial class CoporateWindow : Window
    {
        public static readonly StyledProperty<object> CoporateContentProperty = AvaloniaProperty.Register<CoporateWindow, object>(nameof(CoporateContent));
        private Window ParentWindow => (Window)Owner;

        public object CoporateContent
        {
            get { return GetValue(CoporateContentProperty); }
            set { SetValue(CoporateContentProperty, value); }
        }

        public CoporateWindow()
        {
            DataContext = this;

            Opened += OnOpened;

            // this.WindowState = WindowState.FullScreen;
            this.SystemDecorations = SystemDecorations.None;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnOpened(object sender, EventArgs e)
        {
            CenterDialog();
        }

        private void CenterDialog()
        {
            var x = ParentWindow.Position.X + (ParentWindow.Bounds.Width - Width) / 2;
            var y = ParentWindow.Position.Y + (ParentWindow.Bounds.Height - Height);

            Position = new PixelPoint((int)x, (int)y);
        }

    }
}
