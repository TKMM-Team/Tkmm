using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public partial class TkModStorage : ObservableObject, IModStorage
{
    private readonly ObservableCollection<ITkMod> _mods = [];
    private readonly ObservableCollection<ITkProfile> _profiles = [];
    
    public IList<ITkMod> Mods => _mods;

    public IList<ITkProfile> Profiles => _profiles;

    [ObservableProperty]
    private ITkProfile? _currentProfile;
}