using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Core.Models;
using Tkmm.Core.Studio.Models;
using Tkmm.Models.Mvvm;

namespace Tkmm.Core.Studio;

public sealed partial class TkProject : TkItem
{
    [ObservableProperty]
    private ExportLocations _exportLocations = [];
    
    [ObservableProperty]
    [property: JsonIgnore]
    private ObservableCollection<TkProjectOptionGroup> _groups = [];

    public async ValueTask<ITkMod> Export(IModWriter writer)
    {
        throw new NotImplementedException();
    }

    public async ValueTask Save()
    {
        throw new NotImplementedException();
    }
}