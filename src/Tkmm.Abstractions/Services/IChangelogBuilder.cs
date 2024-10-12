namespace Tkmm.Abstractions.Services;

public interface IChangelogBuilder
{
    ValueTask<bool> LogChanges(ArraySegment<byte> input, in TkFileInfo info, Func<Stream> getOutput,
        CancellationToken ct = default);

    bool IsKnownFile(in TkFileInfo fileInfo);
}