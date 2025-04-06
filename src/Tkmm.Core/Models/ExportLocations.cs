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
        RemoveDuplicatesAndInvalid();
        await SymlinkHelper.CreateMany(GetValidTargets(this), TKMM.MergedOutputFolder);
    }

    private void RemoveDuplicatesAndInvalid()
    {
        HashSet<string> seenPaths = new(StringComparer.OrdinalIgnoreCase);
        var toRemove = new List<ExportLocation>();

        foreach (ExportLocation location in this.Where(x => x.IsEnabled)) {
            string path = location.SymlinkPath;

            if (string.Equals(path, TKMM.MergedOutputFolder, StringComparison.OrdinalIgnoreCase) || !seenPaths.Add(path)) {
                toRemove.Add(location);
                continue;
            }

            if (Directory.Exists(path) && Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any()) {
                location.IsEnabled = false;
                TkLog.Instance.LogWarning(
                    "Export location '{ExportLocationPath}' was omitted and disabled because the folder contained contents.",
                    path);
            }
        }

        foreach (var location in toRemove) {
            Remove(location);
        }
    }

    private static IEnumerable<string> GetValidTargets(IEnumerable<ExportLocation> locations)
    {
        return locations.Where(x => x.IsEnabled).Select(x => x.SymlinkPath);
    }
}