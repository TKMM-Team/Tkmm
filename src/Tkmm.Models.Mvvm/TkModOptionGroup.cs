using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public sealed partial class TkModOptionGroup : TkItem, ITkModOptionGroup
{
    private readonly ObservableCollection<ITkModOption> _options = [];
    private readonly ObservableCollection<ITkModOption> _defaultSelectedOptions = [];
    private readonly ObservableCollection<ITkModDependency> _dependencies = [];
    
    [ObservableProperty]
    private OptionGroupType _type;
    
    [ObservableProperty]
    private object? _icon;
    
    public IList<ITkModOption> Options => _options; 
    
    public IList<ITkModOption> DefaultSelectedOptions => _defaultSelectedOptions; 
    
    public IList<ITkModDependency> Dependencies => _dependencies; 
}