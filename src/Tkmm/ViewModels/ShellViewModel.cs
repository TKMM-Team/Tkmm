using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;

namespace Tkmm.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isFirstTimeSetup = true;

    public ShellViewModel()
    {
        IsFirstTimeSetup = !Config.Shared.ConfigExists();
    }
}
