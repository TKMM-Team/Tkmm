namespace Tkmm.Abstractions.IO;

public interface IModReader
{
    ValueTask<ITkMod?> ReadMod<T>(T? input, Stream? stream = null, CancellationToken ct = default) where T : class
        => ReadMod(input, stream, predefinedModId: default, ct);
    
    ValueTask<ITkMod?> ReadMod<T>(T? input, Stream? stream = null, Ulid predefinedModId = default,
        CancellationToken ct = default) where T : class;
    
    bool IsKnownInput<T>(T? input) where T : class;
}