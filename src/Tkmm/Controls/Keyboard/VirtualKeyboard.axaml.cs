
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Tkmm.Controls.Keyboard.Layout;

namespace Tkmm.Controls.Keyboard;

public enum VirtualKeyboardState
{
    Default,
    Shift,
    Capslock,
    AltCtrl
}

public partial class VirtualKeyboard : UserControl
{
    private static List<Type> Layouts { get; } = new List<Type>();
    private static Func<Type> DefaultLayout { get; set; }

    public static void AddLayout<TLayout>() where TLayout : KeyboardLayout => Layouts.Add(typeof(TLayout));

    public static void SetDefaultLayout(Func<Type> getDefaultLayout) => DefaultLayout = getDefaultLayout;

    public static async Task<string?> ShowDialog(TextInputOptions options, Window? owner = null)
    {
        var keyboard = new VirtualKeyboard();
        return null;
    }

    public TextBox TextBox_ { get; }
    public Button AcceptButton_ { get; }
    public Button CloseButton_ { get; }
    public string targetLayout { get; set; }
    public TransitioningContentControl TransitioningContentControl_ { get; }
    public TextBox source { get; set; }
    public IObservable<VirtualKeyboardState> KeyboardStateStream => _keyboardStateStream;
    private readonly BehaviorSubject<VirtualKeyboardState> _keyboardStateStream;

    public IReactiveCommand CloseCommand { get; }

    public VirtualKeyboard()
    {
        InitializeComponent();
        TextBox_ = this.Get<TextBox>("TextBox");
        TransitioningContentControl_ = this.Get<Avalonia.Controls.TransitioningContentControl>("TransitioningContentControl");
        AcceptButton_ = this.Get<Button>("AcceptButton");
        CloseButton_ = this.Get<Button>("CloseButton");

        AcceptButton_.AddHandler(Button.ClickEvent, acceptClicked);
        CloseButton_.AddHandler(Button.ClickEvent, closeClicked);
        CloseCommand = ReactiveCommand.Create(() => { Close(); });
        Initialized += async (sender, args) =>
        {
            
            if(targetLayout == null)
            {
                TransitioningContentControl_.Content = Activator.CreateInstance(DefaultLayout.Invoke());
            }
            else
            {
                var layout = Layouts.FirstOrDefault(x => x.Name.ToLower().Contains(targetLayout.ToLower()));
                if (layout != null)
                {
                    TransitioningContentControl_.Content = Activator.CreateInstance(layout);
                }
                else
                {
                    TransitioningContentControl_.Content = Activator.CreateInstance(DefaultLayout.Invoke());
                }
            }
        };

        KeyDown += (sender, args) =>
        {
            TextBox_.Focus();
            if (args.Key == Key.Escape)
            {
                TextBox_.Text = "";
            }
            else if (args.Key == Key.Enter)
            {
                
                source.Text = TextBox_.Text;
                
                Close();
            }
        };
        _keyboardStateStream = new BehaviorSubject<VirtualKeyboardState>(VirtualKeyboardState.Default);
    }

    private void closeClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    public void Close()
    {
        
        
        TextBox_.Text = "";
        IsVisible = false;
        ((Control)this.Parent).IsVisible = false;
    }

    public void ShowKeyboard(TextBox source)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBox_.Focus();
        });
        TextBox_.Focus();
        this.source = source;

        this.TextBox_.PasswordChar = source.PasswordChar;
        this.TextBox_.Text = source.Text == null ? "" : source.Text;
        this.IsVisible = true;
        ((Control)this.Parent).IsVisible = true;
        TransitioningContentControl_.Content = Activator.CreateInstance(DefaultLayout.Invoke());
    }
    public void ShowKeyboard(TextBox source, Type layout)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBox_.Focus();
        });

        TextBox_.Focus();
        this.source = source;
        this.TextBox_.PasswordChar = source.PasswordChar;
        this.TextBox_.Text = source.Text == null ? "" : source.Text;

        TextBox_.CaretIndex = TextBox_.Text.Length;
        this.IsVisible = true;
        ((Control)this.Parent).IsVisible = true;
        TransitioningContentControl_.Content = Activator.CreateInstance(layout);
    }


    private void acceptClicked(object? sender, RoutedEventArgs e)
    {
        
        source.Text = TextBox_.Text;
        
        Close();
        
    }

    public void ProcessText(string text)
    {
        TextBox_.Focus();

        if(TextBox_.CaretIndex <= TextBox_.Text.Length)
        {
            TextBox_.Text = TextBox_.Text.Insert(TextBox_.CaretIndex, text);
            TextBox_.CaretIndex = Math.Clamp(TextBox_.CaretIndex + 1 ,0, TextBox_.Text.Length);
        }

        TextBox_.Focus();

        if (_keyboardStateStream.Value == VirtualKeyboardState.Shift)
        {
            _keyboardStateStream.OnNext(VirtualKeyboardState.Default);
        }
    }

    public void ProcessKey(Key key)
    {
        if (key == Key.LeftShift || key == Key.RightShift)
        {
            if (_keyboardStateStream.Value == VirtualKeyboardState.Shift)
            {
                _keyboardStateStream.OnNext(VirtualKeyboardState.Default);
            }
            else
            {
                _keyboardStateStream.OnNext(VirtualKeyboardState.Shift);
            }
        }
        else if (key == Key.RightAlt)
        {
            if (_keyboardStateStream.Value == VirtualKeyboardState.AltCtrl)
            {
                _keyboardStateStream.OnNext(VirtualKeyboardState.Default);
            }
            else
            {
                _keyboardStateStream.OnNext(VirtualKeyboardState.AltCtrl);
            }
        }
        else if (key == Key.CapsLock)
        {
            if (_keyboardStateStream.Value == VirtualKeyboardState.Capslock)
            {
                _keyboardStateStream.OnNext(VirtualKeyboardState.Default);
            }
            else
            {
                _keyboardStateStream.OnNext(VirtualKeyboardState.Capslock);
            }
        }
        else
        {
            if (key == Key.Clear)
            {
                TextBox_.Text = "";
                TextBox_.Focus();
            }
            else if (key == Key.Enter || key == Key.ImeAccept)
            {
                source.Text = TextBox_.Text;
                SimulateEnterKeyPress(source);
                Close();
            }
            else if (key == Key.Help)
            {
                _keyboardStateStream.OnNext(VirtualKeyboardState.Default);
                if (TransitioningContentControl_.Content is KeyboardLayout layout)
                {
                    var index = Layouts.IndexOf(layout.GetType());
                    if (Layouts.Count - 1 > index)
                    {
                        TransitioningContentControl_.Content = Activator.CreateInstance(Layouts[index + 1]);
                    }
                    else
                    {
                        TransitioningContentControl_.Content = Activator.CreateInstance(Layouts[0]);
                    }
                }
            }
            else if(key == Key.Back)
            {

                if(TextBox_.Text != null && TextBox_.CaretIndex <= TextBox_.Text.Length && TextBox_.CaretIndex > 0)
                {
                    int dd = 0;
                    if(TextBox_.CaretIndex !=  TextBox_.Text.Length)
                    {
                        dd = TextBox_.CaretIndex - 1;
                    }

                    TextBox_.Text = TextBox_.Text.Remove(TextBox_.CaretIndex-1, 1);
                    if(dd != 0)
                    {
                        TextBox_.CaretIndex =dd;
                    }
                    
                    
                }
                TextBox_.Focus();

            }
            else if(key == Key.BrowserFavorites)
            {
                _keyboardStateStream.OnNext(VirtualKeyboardState.Default);

                if(TransitioningContentControl_.Content is VirtualKeyboardLayoutNumpad)
                {
                    TransitioningContentControl_.Content = Activator.CreateInstance(DefaultLayout());
                }
                else
                {
                    TransitioningContentControl_.Content = Activator.CreateInstance(typeof(VirtualKeyboardLayoutNumpad));
                }                    
            }
            else if (key == Key.Left)
            {
                TextBox_.CaretIndex = Math.Clamp(TextBox_.CaretIndex - 1, 0, TextBox_.Text.Length);
                TextBox_.Focus();
            }
            else if (key == Key.Right)
            {
                TextBox_.CaretIndex = Math.Clamp(TextBox_.CaretIndex + 1,0, TextBox_.Text.Length);
                TextBox_.Focus();
            }
            else
            {
                TextBox_.Focus();
            }
        }
    }

    private void SimulateEnterKeyPress(Control target)
    {
        var keyEventArgs = new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Enter,
            Source = target,
            KeyModifiers = KeyModifiers.None
        };
        target.RaiseEvent(keyEventArgs);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}