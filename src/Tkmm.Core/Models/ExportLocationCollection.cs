using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Tkmm.Core.Models;

public partial class ExportLocationCollection : ObservableCollection<ExportLocation>
{
    [RelayCommand]
    private void New()
    {
        Add(new());
    }

    [RelayCommand]
    private void Delete(ExportLocation target)
    {
        Remove(target);
    }
}
