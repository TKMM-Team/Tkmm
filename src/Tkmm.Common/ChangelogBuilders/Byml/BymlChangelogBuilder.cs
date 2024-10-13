using Tkmm.Abstractions;
using Tkmm.Abstractions.Services;

// ReSharper disable once CheckNamespace
namespace Tkmm.Common.ChangelogBuilders;

public sealed class BymlChangelogBuilder : IChangelogBuilder
{
    public static readonly BymlChangelogBuilder Instance = new();
    
    public ValueTask LogChanges(string canonical, TkFileAttributes attributes, ArraySegment<byte> input, Func<ValueTask<Stream>> getOutput, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public bool IsKnownFile(in TkFileInfo fileInfo)
    {
        return fileInfo.Extension is ".byml" or ".bgyml";
    }
}