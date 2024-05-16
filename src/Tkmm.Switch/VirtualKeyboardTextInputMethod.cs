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

        public async Task SetActive(GotFocusEventArgs e)
        {
            if (!_isOpen )
            {
                
                _isOpen = true;
                var oskReturn  = await VirtualKeyboard.ShowDialog(_textInputOptions, this.root);
                
                if(e.Source.GetType() == typeof(TextBox))
                {
                    ((TextBox)e.Source).Text = oskReturn;
                   
                }

                _isOpen = false;
                _textInputOptions = null;

                if (this.root != null)
                {
                   root!.Focus();
                }
                else if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                   desktop.MainWindow.Focus();
                }

                e.Handled = true;
                
            }
        }
    }