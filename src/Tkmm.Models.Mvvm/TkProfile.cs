using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public sealed partial class TkProfile : TkItem, ITkProfile
{
    private readonly ObservableCollection<ITkProfileMod> _mods = [];
    
    public Ulid Id { get; } = Ulid.NewUlid();

    [ObservableProperty]
    private ITkProfileMod? _selected;
    
    public IList<ITkProfileMod> Mods => _mods;
}