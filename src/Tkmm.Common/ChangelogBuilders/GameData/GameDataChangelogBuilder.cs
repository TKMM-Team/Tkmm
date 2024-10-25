using Tkmm.Abstractions;
using Tkmm.Abstractions.Services;

namespace Tkmm.Common.ChangelogBuilders.GameData;

public sealed class GameDataChangelogBuilder : IChangelogBuilder
{
    public ValueTask LogChanges(string canonical, TkFileAttributes attributes, ArraySegment<byte> input, Func<ValueTask<Stream>> getOutput, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public bool IsKnownFile(in TkFileInfo fileInfo)
    {
        throw new NotImplementedException();
    }
}