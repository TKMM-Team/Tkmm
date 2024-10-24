using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Models.Mvvm;

namespace Tkmm.Core.Studio.Models;

public partial class TkProjectOption : TkItem
{
    [ObservableProperty]
    private string? _targetFolder;
    
    
}