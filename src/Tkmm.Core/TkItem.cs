using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.Common;

namespace Tkmm.Core;

internal partial class TkItem : ObservableObject, ITkItem
{
    public Ulid Id { get; internal set; } = Ulid.NewUlid();

    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private IThumbnail? _thumbnail;
}