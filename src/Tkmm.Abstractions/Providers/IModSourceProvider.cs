using Tkmm.Abstractions.IO;

namespace Tkmm.Abstractions.Providers;

public interface IModSourceProvider
{
    /// <summary>
    /// Attempt to retreive a <see cref="IModSource"/> from the provided <paramref name="input"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The located <see cref="IModSource"/> or null if none could be found.</returns>
    ValueTask<IModSource?> GetSource(object? input);
}