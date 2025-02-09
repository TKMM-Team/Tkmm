using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Core.Models;

public sealed partial class PathCollection : ObservableCollection<PathCollectionItem>, IEnumerable<string>
{
    [RelayCommand]
    private void New()
    {
        Add(new PathCollectionItem());
    }

    [RelayCommand]
    private void Delete(PathCollectionItem target)
    {
        Remove(target);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => Items.Select(x => x.Target).GetEnumerator();
}