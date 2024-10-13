using Tkmm.Abstractions;
using Tkmm.Abstractions.Services;

// ReSharper disable once CheckNamespace
namespace Tkmm.Common.ChangelogBuilders;

public sealed class MsbtChangelogBuilder : IChangelogBuilder
{
    public static readonly MsbtChangelogBuilder Instance = new();
    
    public ValueTask<bool> LogChanges(string canonical, TkFileAttributes attributes, ArraySegment<byte> input, Func<ValueTask<Stream>> getOutput, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public bool IsKnownFile(in TkFileInfo fileInfo)
    {
        return fileInfo.Extension is ".msbt";
    }
}