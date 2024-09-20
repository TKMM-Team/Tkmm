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
    /// Imports the provided <paramref name="mod"/> into the <see cref="CurrentProfile"/>.
    /// </summary>
    /// <param name="mod">The <see cref="ITkMod"/> to import.</param>
    /// <param name="ct"></param>
    virtual ValueTask Import(ITkMod mod, CancellationToken ct = default) => Import(mod, CurrentProfile, ct);

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