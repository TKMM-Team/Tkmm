using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public class TkModChangelog : TkItem, ITkModChangelog
{
    public Ulid Id { get; } = Ulid.NewUlid();
    
    public string? RelativePath { get; init; }

    public IDictionary<string, ChangelogEntry> Manifest { get; } = new Dictionary<string, ChangelogEntry>();

    public IList<TkPatch> Patches { get; } = [];

    public IList<string> SubSdkFiles { get; } = [];

    public IList<string> Cheats { get; } = [];
}