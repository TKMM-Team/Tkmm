using Tkmm.Abstractions;
using Tkmm.Abstractions.Services;

namespace Tkmm.Common.ChangelogBuilders.Mals;

public class MalsChangelogBuilder : IChangelogBuilder
{
    public static readonly MalsChangelogBuilder Instance = new();
    
    public ValueTask LogChanges(string canonical, TkFileAttributes attributes, ArraySegment<byte> input, Func<ValueTask<Stream>> getOutput, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public bool IsKnownFile(in TkFileInfo fileInfo)
    {
        return fileInfo.Extension is ".sarc"
            && fileInfo.Canonical.Length > 4
            && fileInfo.Canonical[..4] is "Mals";
    }
}