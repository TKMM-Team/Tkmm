using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Abstractions;

namespace Tkmm.Core;

public sealed partial class TkMod : TkModChangelog, ITkMod
{
    private readonly ObservableCollection<ITkModContributor> _contributors = [];
    private readonly ObservableCollection<ITkModOptionGroup> _optionGroups = [];
    private readonly ObservableCollection<ITkModDependency> _dependencies = [];

    [ObservableProperty]
    private string _author = string.Empty;

    public IList<ITkModContributor> Contributors => _contributors;

    [ObservableProperty]
    private string _version = string.Empty;

    public IList<ITkModOptionGroup> OptionGroups => _optionGroups;

    public IList<ITkModDependency> Dependencies => _dependencies;

    public ITkProfileMod GetProfileMod()
    {
        return new TkProfileMod(this);
    }
}