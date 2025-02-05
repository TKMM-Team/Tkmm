using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using TkSharp.Core;

namespace Tkmm.Core.Models;

public sealed partial class ExportLocations : ObservableCollection<ExportLocation>
{
    [RelayCommand]
    private void New()
    {
        Add(new ExportLocation());
    }

    [RelayCommand]
    private void Delete(ExportLocation target)
    {
        Remove(target);
    }

    public async ValueTask Create()
    {
        await SymlinkHelper.CreateMany(GetValidTargets(this), TKMM.MergedOutputFolder);
    }

    private static IEnumerable<string> GetValidTargets(IEnumerable<ExportLocation> locations)
    {
        foreach (ExportLocation location in locations.Where(x => x.IsEnabled)) {
            string path = location.SymlinkPath;
            if (Directory.Exists(path) && Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any()) {
                location.IsEnabled = false;
                TkLog.Instance.LogWarning(
                    "Export location '{ExportLocationPath}' was omitted and disabled because the folder contained contents.",
                    path);
                continue;
            }
            
            yield return path;
        }
    }
}