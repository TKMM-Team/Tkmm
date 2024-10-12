using Tkmm.Abstractions.Services;

namespace Tkmm.Abstractions.Providers;

public interface IMergerProvider
{
    /// <summary>
    /// Attempt to retreive a <see cref="IMerger"/> from the <paramref name="canonical"/> file name.
    /// </summary>
    /// <param name="canonical"></param>
    /// <returns>The located <see cref="IMerger"/> or null if none could be found.</returns>
    IMerger? GetMerger(in ReadOnlySpan<char> canonical);
}