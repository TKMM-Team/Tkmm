namespace Tkmm.Abstractions.IO;

public interface IModSource
{
    /// <summary>
    /// The path to the romfs root folder contained in the <see cref="RomfsFiles"/>.  
    /// </summary>
    string RomfsPath { get; }

    /// <summary>
    /// The files contained in this mod source.
    /// </summary>
    IEnumerable<(object FileEntry, string FileName)> RomfsFiles { get; }

    /// <summary>
    /// Open a stream to the requested <paramref name="romfsFileInput"/>.
    /// </summary>
    /// <param name="romfsFileInput">The file to open.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<(Stream Stream, long StreamLength)> OpenRead(object romfsFileInput, CancellationToken ct = default);
    
    /// <summary>
    /// Determine if the provided input can be read by this <see cref="IModSource"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    ValueTask<bool> IsKnownInput<T>(T? input) where T : class;
}