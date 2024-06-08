namespace Tkmm.Core.Helpers.Operations;

public static class DirectoryOperations
{
    private const string ZS_EXT = ".zs";

    public static void DeleteContents(string src, bool recursive)
    {
        foreach (string directory in Directory.EnumerateDirectories(src)) {
            Directory.Delete(directory, recursive);
        }
    }

    public static void CopyDirectory(string src, string dst, bool overwrite = false)
    {
        Directory.CreateDirectory(dst);

        foreach (var file in Directory.EnumerateFiles(src)) {
            File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), overwrite);
        }

        foreach (var folder in Directory.EnumerateDirectories(src)) {
            string folderName = Path.GetFileName(folder);
            string dstFolder = Path.Combine(dst, folderName);
            CopyDirectory(folder, dstFolder, overwrite);
        }
    }

    public static void CopyDirectory(string src, string dst, List<string> excludeFiles, List<string> excludeFolders, bool overwrite = false)
    {
        foreach (var file in Directory.EnumerateFiles(src)) {
            string ext = Path.GetExtension(file);
            if (ext == ZS_EXT) {
                ext = Path.GetExtension(file[..^3]);
            }

            if (!excludeFiles.Contains(ext)) {
                Directory.CreateDirectory(dst);
                File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), overwrite);
            }
        }

        foreach (var folder in Directory.EnumerateDirectories(src)) {
            string folderName = Path.GetFileName(folder);
            if (!excludeFolders.Contains(folderName)) {
                string dstFolder = Path.Combine(dst, folderName);
                CopyDirectory(folder, dstFolder, excludeFiles, excludeFolders, overwrite);
            }
        }
    }
}
