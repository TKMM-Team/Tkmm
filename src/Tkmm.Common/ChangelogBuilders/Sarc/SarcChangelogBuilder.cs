using Tkmm.Abstractions;
using Tkmm.Abstractions.Services;

// ReSharper disable once CheckNamespace
namespace Tkmm.Common.ChangelogBuilders;

internal sealed class SarcChangelogBuilder : IChangelogBuilder
{
    public static readonly SarcChangelogBuilder Instance = new();

    public ValueTask LogChanges(string canonical, TkFileAttributes attributes, ArraySegment<byte> input,
        Func<ValueTask<Stream>> getOutput, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public bool IsKnownFile(in TkFileInfo fileInfo)
    {
        return fileInfo.Extension is ".bfarc" or ".bkres" or ".blarc" or ".genvb" or ".pack" or ".ta";
    }
}