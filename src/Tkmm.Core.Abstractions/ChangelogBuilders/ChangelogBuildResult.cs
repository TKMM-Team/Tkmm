namespace Tkmm.Core.Abstractions.ChangelogBuilders;

public sealed class ChangelogBuildResult<TChangelog>(bool isVanilla, TChangelog changelog)
{
    public bool IsVanilla { get; } = isVanilla;

    public TChangelog Changelog { get; } = changelog;
}