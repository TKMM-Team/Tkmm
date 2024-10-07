using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Abstractions;

namespace Tkmm.Core;

internal sealed partial class TkProfileMod(ITkMod mod) : ObservableObject, ITkProfileMod
{
    [ObservableProperty]
    private ITkMod _mod = mod;
    
    [ObservableProperty]
    private bool _isEnabled;
    
    [ObservableProperty]
    private bool _isEditingOptions;
}