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
    
    public void Reset(string mergeOutputFolder)
    {
        foreach (var target in GetValidTargets(this).Where(path => new DirectoryInfo(path).LinkTarget is not null)) {
            Directory.Delete(target);
        }
        
        SymlinkHelper.CreateMany(GetValidTargets(this), mergeOutputFolder);
    }

    public void Create()
    {
        RemoveDuplicatesAndInvalid();
        SymlinkHelper.CreateMany(GetValidTargets(this), TKMM.MergedOutputFolder);
    }

    private void RemoveDuplicatesAndInvalid()
    {
        HashSet<string> seenPaths = new(StringComparer.OrdinalIgnoreCase);
        var toRemove = new List<ExportLocation>();

        foreach (var location in this.Where(x => x.IsEnabled)) {
            var path = location.SymlinkPath;

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

            location.SymlinkPath = path.TrimEnd('\\', '/');
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