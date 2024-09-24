using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Abstractions.Common;

namespace Tkmm.Core.Models;

public partial class TkThumbnail : ObservableObject, IThumbnail
{
    public const string DEFAULT = ".thumbnail";
    
    [ObservableProperty]
    private string _thumbnailPath = DEFAULT;
    
    [ObservableProperty]
    private object? _thumbnail;
}