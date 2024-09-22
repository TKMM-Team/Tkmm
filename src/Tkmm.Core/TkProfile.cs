using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Common;

namespace Tkmm.Core;

internal sealed partial class TkProfile : ObservableObject, ITkProfile
{
    private readonly ObservableCollection<ITkProfileMod> _mods = [];
    
    public Ulid Id { get; } = Ulid.NewUlid();

    [ObservableProperty]
    private string _name = SystemMsg.DefaultProfileName;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private IThumbnail? _thumbnail;

    public IList<ITkProfileMod> Mods => _mods;
}