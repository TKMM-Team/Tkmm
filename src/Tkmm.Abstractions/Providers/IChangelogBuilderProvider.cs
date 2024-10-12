using Tkmm.Abstractions.Services;

namespace Tkmm.Abstractions.Providers;

public interface IChangelogBuilderProvider
{
    /// <summary>
    /// Attempt to retreive a <see cref="IChangelogBuilder"/> from the provided <paramref name="fileInfo"/>.
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns>The located <see cref="IChangelogBuilder"/> or null if none could be found.</returns>
    IChangelogBuilder? GetChangelogBuilder(in TkFileInfo fileInfo);
}