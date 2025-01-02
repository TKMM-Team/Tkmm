using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.VirtualKeyboard;

public partial class VirtualKeyboardLayout : TemplatedControl
{
    private readonly HashSet<VirtualKey> _registered = [];
    
    public static readonly StyledProperty<bool> AcceptsReturnProperty = AvaloniaProperty.Register<VirtualKeyboard, bool>(nameof(AcceptsReturn));
    public static readonly StyledProperty<bool> IsShiftEnabledProperty = AvaloniaProperty.Register<VirtualKeyboard, bool>(nameof(IsShiftEnabled));

    public bool IsShiftEnabled {
        get => GetValue(IsShiftEnabledProperty);
        set => SetValue(IsShiftEnabledProperty, value);
    }
    
    public bool AcceptsReturn {
        get => GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    protected VirtualKeyboardLayout()
    {
        PropertyChanged += (_, e) => {
            if (e.Property.Name != nameof(IsShiftEnabled)) {
                return;
            }
            
            UpdateKeyContents(IsShiftEnabled);
        };
    }

    public void RegisterKey(VirtualKey key)
    {
        _registered.Add(key);
    }
    
    [RelayCommand]
    private void Backspace()
    {
        if (this.FindAncestorOfType<VirtualKeyboard>() is not VirtualKeyboard keyboard) {
            return;
        }

        if (keyboard.VirtualText is null || keyboard.TextPosition < 1) {
            return;
        }

        int pos = keyboard.TextPosition -= 1;
        
        keyboard.VirtualText = string.Create(keyboard.VirtualText.Length - 1, keyboard.VirtualText.AsSpan(), (dst, src) => {
            int offset = -1;
            for (int i = 0; i < src.Length; i++) {
                if (i == pos) {
                    continue;
                }
                
                dst[++offset] = src[i];
            }
        });
    }

    [RelayCommand]
    private void MoveHome()
    {
        if (this.FindAncestorOfType<VirtualKeyboard>() is not VirtualKeyboard keyboard) {
            return;
        }
        
        if (keyboard.TextPosition < 1) {
            return;
        }

        if (!AcceptsReturn) {
            keyboard.TextPosition = 0;
            return;
        }
        
        ReadOnlySpan<char> span = keyboard.VirtualText.AsSpan(); 
        int lineFeedIndex = span[..keyboard.TextPosition].LastIndexOf('\n');
        keyboard.TextPosition = lineFeedIndex + 1;
    }

    [RelayCommand]
    private void MoveEnd()
    {
        if (this.FindAncestorOfType<VirtualKeyboard>() is not VirtualKeyboard keyboard) {
            return;
        }

        if (keyboard.VirtualText?.Length <= keyboard.TextPosition || keyboard.VirtualText is null) {
            return;
        }
        
        if (!AcceptsReturn) {
            keyboard.TextPosition = keyboard.VirtualText.Length;
            return;
        }
        
        keyboard.TextPosition++;
    }

    [RelayCommand]
    private void MoveLeft()
    {
        if (this.FindAncestorOfType<VirtualKeyboard>() is not VirtualKeyboard keyboard) {
            return;
        }
        
        if (keyboard.TextPosition < 1) {
            return;
        }
        
        keyboard.TextPosition--;
    }

    [RelayCommand]
    private void MoveRight()
    {
        if (this.FindAncestorOfType<VirtualKeyboard>() is not VirtualKeyboard keyboard) {
            return;
        }

        if (keyboard.VirtualText?.Length <= keyboard.TextPosition) {
            return;
        }
        
        keyboard.TextPosition++;
    }

    private void UpdateKeyContents(bool isEnabled)
    {
        foreach (VirtualKey key in _registered) {
            key.Content = isEnabled ? key.ShiftKey : key.Key;
        }
    }
}