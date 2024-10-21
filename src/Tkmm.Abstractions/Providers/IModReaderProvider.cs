using Tkmm.Abstractions.IO;

namespace Tkmm.Abstractions.Providers;

public interface IModReaderProvider
{
    /// <summary>
    /// Attempt to retreive a <see cref="IModReader"/> from the provided <paramref name="input"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The located <see cref="IModReader"/> or null if none could be found.</returns>
    IModReader? GetReader<T>(T? input) where T : class;
    
    /// <summary>
    /// Determine if the provided <paramref name="input"/> can be read.
    /// </summary>
    /// <param name="input"></param>
    bool CanRead<T>(T? input) where T : class;
}