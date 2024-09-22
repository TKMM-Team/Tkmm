using Tkmm.Core.Abstractions.Parsers;

namespace Tkmm.Core.Abstractions;

public interface ITkModManager
{
    /// <summary>
    /// The master collection of installed mods.
    /// </summary>
    IList<ITkMod> Mods { get; }

    /// <summary>
    /// The current profile in use by the frontend.
    /// </summary>
    ITkProfile CurrentProfile { get; set; }

    /// <summary>
    /// The collection of custom profiles.
    /// </summary>
    IList<ITkProfile> Profiles { get; }

    /// <summary>
    /// Create an <see cref="ITkMod"/> from an <paramref name="argument"/> and optional <paramref name="stream"/>.
    /// </summary>
    /// <param name="argument">The input argument, validated by the registered <see cref="ITkModParser"/>'s.</param>
    /// <param name="stream">The input data stream.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<ITkMod?> Create(string argument, Stream? stream = null, CancellationToken ct = default);

    /// <summary>
    /// Imports the provided <paramref name="mod"/> into the <see cref="CurrentProfile"/>.
    /// </summary>
    /// <param name="mod">The <see cref="ITkMod"/> to import.</param>
    /// <param name="ct"></param>
    ValueTask Import(ITkMod mod, CancellationToken ct = default) => Import(mod, CurrentProfile, ct);

    /// <summary>
    /// Imports the provided <paramref name="mod"/> into the provided <paramref name="profile"/>.
    /// </summary>
    /// <param name="mod">The <see cref="ITkMod"/> to import.</param>
    /// <param name="profile"></param>
    /// <param name="ct"></param>
    ValueTask Import(ITkMod mod, ITkProfile profile, CancellationToken ct = default)
    {
        profile.Mods.Add(mod.GetProfileMod());
        Mods.Add(mod);
        return ValueTask.CompletedTask;
    }

    ValueTask Merge(CancellationToken ct = default) => Merge(CurrentProfile, ct);
    
    ValueTask Merge(ITkProfile profile, CancellationToken ct = default);

    ValueTask InitializeAsync();
}