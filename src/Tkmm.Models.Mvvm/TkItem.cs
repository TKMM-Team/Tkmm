using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public partial class TkItem : ObservableObject, ITkItem
{
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ITkThumbnail? _thumbnail;
}