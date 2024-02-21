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

    public Task Merge(IModItem[] mods, string output)
    {
        foreach (var item in mods) {
            CopyContents(item.SourceFolder, output);

            if (item is Mod mod) {
                foreach (var group in mod.OptionGroups.Reverse()) {
                    foreach (var option in group.SelectedOptions.Reverse()) {
                        CopyContents(option.SourceFolder, output);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void CopyContents(string sourceFolder, string output)
    {
        foreach (var folder in TotkConfig.FileSystemFolders) {
            string srcFolder = Path.Combine(sourceFolder, folder);
            if (Directory.Exists(srcFolder)) {
                DirectoryOperations.CopyDirectory(srcFolder, Path.Combine(output, folder), ExcludeFiles, ToolHelper.ExcludeFolders, true);
            }
        }
    }
}
