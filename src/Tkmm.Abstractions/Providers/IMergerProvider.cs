using Tkmm.Abstractions.Services;

namespace Tkmm.Abstractions.Providers;

public interface IMergerProvider
{
    /// <summary>
    /// Attempt to retreive a <see cref="IMerger"/> from the provided <paramref name="fileInfo"/>.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns>The located <see cref="IMerger"/> or null if none could be found.</returns>
    IMerger? GetMerger(in TkFileInfo fileInfo);
}