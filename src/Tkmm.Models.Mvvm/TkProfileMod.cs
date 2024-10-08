using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public sealed partial class TkProfileMod(ITkMod mod) : ObservableObject, ITkProfileMod
{
    [ObservableProperty]
    private ITkMod _mod = mod;
    
    [ObservableProperty]
    private bool _isEnabled;
    
    [ObservableProperty]
    private bool _isEditingOptions;
}