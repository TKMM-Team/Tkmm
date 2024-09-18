namespace Tkmm.Core.Abstractions;

public interface ITkMod : ITkItem, ITkModChangelog
{
    string Author { get; set; }

    IList<ITkModContributor> Contributors { get; }

    string Version { get; set; }

    IList<ITkModOptionGroup> OptionGroups { get; }

    IList<ITkModDependency> Dependencies { get; }

    ITkProfileMod GetProfileMod();
}