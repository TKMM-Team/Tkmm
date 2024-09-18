namespace Tkmm.Core.Abstractions;

public enum OptionGroupType
{
    Multi,
    MultiRequired,
    Single,
    SingleRequired
}

public interface ITkModOptionGroup
{
    OptionGroupType Type { get; set; }
    string IconName { get; set; }
    IList<ITkModOption> Options { get; set; }
    IList<ITkModOption> SelectedOptions { get; set; }
    IList<ITkModDependency> Dependencies { get; set; }
}