using Tkmm.Core.Generics;
using Tkmm.Core.Models.Mergers.Exefs;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.Mergers;

public class ExefsMerger : IMerger
{
    public Task Merge(IModItem[] mods, string output)
    {
        string[] pchtxtFiles = mods
            .Select(x => Path.Combine(x.SourceFolder, TotkConfig.EXEFS))
            .Where(Directory.Exists)
            .SelectMany(Directory.EnumerateFiles)
            .Where(x => Path.GetExtension(x.AsSpan()) is ".pchtxt")
            .ToArray();

        string[] ipsFiles = mods
            .Select(x => Path.Combine(x.SourceFolder, TotkConfig.EXEFS))
            .Where(Directory.Exists)
            .SelectMany(Directory.EnumerateFiles)
            .Where(x => Path.GetExtension(x.AsSpan()) is ".ips")
            .ToArray();

        if (pchtxtFiles.Length <= 0 && ipsFiles.Length <= 0) {
            return Task.CompletedTask;
        }

        ExePatch patch = new();

        foreach (string ips in ipsFiles) {
            patch.AppendIps(ips);
        }

        foreach (string pchtxt in pchtxtFiles) {
            patch.AppendPchtxt(pchtxt);
        }

        patch.Write(output);

        return Task.CompletedTask;
    }
}
