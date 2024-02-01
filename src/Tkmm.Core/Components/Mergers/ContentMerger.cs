using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;

namespace Tkmm.Core.Components.Mergers;

public class ContentMerger : IMerger
{
    public static List<string> ExcludeFiles => [
        .. ToolHelper.ExcludeFiles,
        ".json",
        ".thumbnail"
    ];

    public Task Merge(IModItem[] mods)
    {
        foreach (var item in mods) {
            CopyContents(item.SourceFolder);

            if (item is Mod mod) {
                foreach (var group in mod.OptionGroups.Reverse()) {
                    foreach (var option in group.Options.Reverse()) {
                        CopyContents(option.SourceFolder);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void CopyContents(string sourceFolder)
    {
        foreach (var folder in TotkConfig.FileSystemFolders) {
            string srcFolder = Path.Combine(sourceFolder, folder);
            if (Directory.Exists(srcFolder)) {
                DirectoryOperations.CopyDirectory(srcFolder, Path.Combine(Config.Shared.MergeOutput, folder), ExcludeFiles, ToolHelper.ExcludeFolders, true);
            }
        }
    }
}
