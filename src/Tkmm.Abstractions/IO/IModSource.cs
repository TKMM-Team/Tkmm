namespace Tkmm.Abstractions.IO;

public interface IModSource
{
    /// <summary>
    /// The files contained in this mod source.
    /// </summary>
    IEnumerable<string> Files { get; }

    /// <summary>
    /// The path to the romfs root folder contained in the <see cref="Files"/>.  
    /// </summary>
    string RomfsPath { get; }

    /// <summary>
    /// Open a stream to the requested <paramref name="file"/>.
    /// </summary>
    /// <param name="file">The file to open.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<(Stream Stream, long StreamLength)> OpenRead(string file, CancellationToken ct = default);
    
    /// <summary>
    /// Determine if the provided input can be read by this <see cref="IModSource"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    ValueTask<bool> IsKnownInput<T>(T? input) where T : class;
}