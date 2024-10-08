namespace Tkmm.Abstractions.IO;

public interface IModSource
{
    /// <summary>
    /// The files contained in this mod source.
    /// </summary>
    List<string> Files { get; }
    
    /// <summary>
    /// Open a stream to the requested <paramref name="file"/>.
    /// </summary>
    /// <param name="file">The file to open.</param>
    /// <returns></returns>
    ValueTask<Stream> OpenRead(string file);
    
    /// <summary>
    /// Determine if the provided input can be read by this <see cref="IModSource"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    ValueTask<bool> IsKnownInput<T>(T? input) where T : class;
}