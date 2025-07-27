using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tkmm.Wizard.Pages;

public class BaseGameSplitTypePageContext : INotifyPropertyChanged
{
    private bool _isSingleFile = true;
    private bool _isSplitFile;

    public bool IsSingleFile
    {
        get => _isSingleFile;
        set {
            if (_isSingleFile == value) {
                return;
            }
            
            _isSingleFile = value;
            
            if (value) {
                IsSplitFile = false;
            }
            
            OnPropertyChanged();
        }
    }

    public bool IsSplitFile
    {
        get => _isSplitFile;
        set {
            if (_isSplitFile == value) {
                return;
            }
            
            _isSplitFile = value;
            
            if (value) {
                IsSingleFile = false;
            }
            
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 