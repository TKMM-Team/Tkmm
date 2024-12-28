using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;

namespace Tkmm.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    public static readonly ShellViewModel Shared = new();
    
    [ObservableProperty]
    private bool _isFirstTimeSetup = true;

    public ShellViewModel()
    {
        IsFirstTimeSetup = !Config.Shared.ConfigExists();
    }
}
