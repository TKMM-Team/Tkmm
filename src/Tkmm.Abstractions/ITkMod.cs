namespace Tkmm.Abstractions;

public interface ITkMod : ITkItem, ITkModChangelog
{
    /// <summary>
    /// The author of this mod.
    /// </summary>
    string Author { get; set; }

    /// <summary>
    /// The contributors of this mod.
    /// </summary>
    IList<ITkModContributor> Contributors { get; }

    /// <summary>
    /// The version of this mod.
    /// </summary>
    string Version { get; set; }

    /// <summary>
    /// The option groups in this mod.
    /// </summary>
    IList<ITkModOptionGroup> OptionGroups { get; }

    /// <summary>
    /// The dependencies of this mod.
    /// </summary>
    IList<ITkModDependency> Dependencies { get; }

    /// <summary>
    /// Get the corresponding <see cref="ITkProfileMod"/> of this <see cref="ITkMod"/>.
    /// </summary>
    /// <returns></returns>
    ITkProfileMod GetProfileMod();
}