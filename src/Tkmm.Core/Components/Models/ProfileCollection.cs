using System.Collections.ObjectModel;

namespace Tkmm.Core.Components.Models;

public class ProfileCollection
{
    public required int CurrentIndex { get; set; }
    public required ObservableCollection<Profile> Profiles { get; set; }
}
