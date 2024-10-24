using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Models.Mvvm;

namespace Tkmm.Core.Studio.Models;

public sealed partial class TkProjectOptionGroup : TkItem
{
    [ObservableProperty]
    private string? _targetFolder;

    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<TkProjectOption> _options = [];
}