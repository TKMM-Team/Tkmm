using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public class TkModChangelog : TkItem, ITkModChangelog
{
    public Ulid Id { get; } = Ulid.NewUlid();

    public IDictionary<string, ChangelogEntry> Manifest { get; } = new Dictionary<string, ChangelogEntry>();
}