namespace Tkmm.Abstractions;

public interface IModStorage
{
    /// <summary>
    /// The master collection of mods.
    /// </summary>
    IList<ITkMod> Mods { get; }

    /// <summary>
    /// The master collection of profiles.
    /// </summary>
    IList<ITkProfile> Profiles { get; }

    /// <summary>
    /// The current profile in use.
    /// </summary>
    ITkProfile CurrentProfile { get; set; }
}