namespace Tkmm.Abstractions.IO;

public abstract class ModSource<T>(string romfsPath) : IModSource where T : notnull
{
    public string RomfsPath { get; } = romfsPath;

    IEnumerable<(object, string)> IModSource.RomfsFiles => RomfsFiles
        .Select(t => ((object)t, GetFileName(t)));
    
    protected abstract IEnumerable<T> RomfsFiles { get; }

    public ValueTask<(Stream Stream, long StreamLength)> OpenRead(object romfsFileInput, CancellationToken ct = default)
        => OpenRead((T)romfsFileInput, ct);

    protected abstract ValueTask<(Stream Stream, long StreamLength)> OpenRead(T input, CancellationToken ct = default);
    
    public abstract ValueTask<bool> IsKnownInput<TSource>(TSource? input) where TSource : class;
    
    protected abstract string GetFileName(T input);
}