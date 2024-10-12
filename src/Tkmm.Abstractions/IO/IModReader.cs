namespace Tkmm.Abstractions.IO;

public interface IModReader
{
    ValueTask<ITkMod?> ReadMod<T>(T? input, Stream? stream = null, ModContext context = default,
        CancellationToken ct = default) where T : class;
    
    bool IsKnownInput<T>(T? input) where T : class;
}