namespace Tkmm.Abstractions;

public enum OptionGroupType
{
    Multi,
    MultiRequired,
    Single,
    SingleRequired
}

public interface ITkModOptionGroup
{
    /// <summary>
    /// The type of option group.
    /// </summary>
    OptionGroupType Type { get; set; }

    /// <summary>
    /// The icon for this option group.
    /// </summary>
    object? Icon { get; set; }

    /// <summary>
    /// The options contained in this option group.
    /// </summary>
    IList<ITkModOption> Options { get; }

    /// <summary>
    /// The default selected options.
    /// </summary>
    IList<ITkModOption> DefaultSelectedOptions { get; }

    /// <summary>
    /// The dependencies of this option group.
    /// </summary>
    IList<ITkModDependency> Dependencies { get; }
}