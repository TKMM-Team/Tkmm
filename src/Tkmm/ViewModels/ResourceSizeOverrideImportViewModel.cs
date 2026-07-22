using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TkSharp.Packaging;

namespace Tkmm.ViewModels;

public sealed partial class ResourceSizeOverrideImportViewModel : ObservableObject
{
    private readonly ResourceSizeOverrideImportEntryViewModel[] _entries;

    public ResourceSizeOverrideImportViewModel(IEnumerable<TkResourceSizeOverrideCandidate> candidates)
    {
        _entries = candidates
            .Select(candidate => new ResourceSizeOverrideImportEntryViewModel(candidate.Canonical, candidate.Size))
            .ToArray();
        ApplyFilter();
    }

    public ObservableCollection<ResourceSizeOverrideImportEntryViewModel> Entries { get; } = [];

    public IEnumerable<ResourceSizeOverrideImportEntryViewModel> SelectedEntries
        => _entries.Where(entry => entry.IsSelected);

    [ObservableProperty]
    private string _search = string.Empty;

    partial void OnSearchChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var entry in Entries) {
            entry.IsSelected = true;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        foreach (var entry in Entries) {
            entry.IsSelected = false;
        }
    }

    private void ApplyFilter()
    {
        Entries.Clear();
        foreach (var entry in _entries.Where(entry =>
                     entry.Canonical.Contains(Search, StringComparison.OrdinalIgnoreCase))) {
            Entries.Add(entry);
        }
    }
}

public sealed partial class ResourceSizeOverrideImportEntryViewModel(string canonical, uint size) : ObservableObject
{
    public string Canonical { get; } = canonical;

    public uint Size { get; } = size;

    [ObservableProperty]
    private bool _isSelected;
}