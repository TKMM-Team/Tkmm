using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Services;

namespace Tkmm.Abstractions.Providers;

public interface IMergerProvider
{
    /// <summary>
    /// Attempt to retreive a <see cref="IMerger"/> from the provided <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>The located <see cref="IMerger"/> or null if none could be found.</returns>
    ValueTask<IMerger?> GetMerger(string fileName);
}