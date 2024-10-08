using Tkmm.Abstractions;
using Tkmm.Models.Mvvm;

namespace Tkmm.Core;

public sealed class ModManager : TkModStorage, IModManager
{
    public ValueTask Initialize(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask Save(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask Merge(ITkProfile profile, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<ITkMod?> Install(object? input, Stream? stream = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<ITkMod?> InstallWithId(object? input, Ulid id, Stream? stream = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Stream> GetModFile(ITkMod targetMod, string manifestFileName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask CreateModFile(ITkMod targetMod, string manifestFileName, Span<byte> data, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}