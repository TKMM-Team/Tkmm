namespace Tkmm.Abstractions.Services;

public interface IChangelogBuilder
{
    ValueTask<bool> LogChanges(string canonical, TkFileAttributes attributes,
        ArraySegment<byte> input, Func<ValueTask<Stream>> getOutput, CancellationToken ct = default);

    bool IsKnownFile(in TkFileInfo fileInfo);
}