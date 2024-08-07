﻿namespace Tkmm.Core.Helpers.Operations;

public static class DirectoryOperations
{
    private const string ZS_EXT = ".zs";
    private const string INI_EXT = ".ini";

    public static void ClearAttributes(string src)
    {
        foreach (string file in Directory.EnumerateFiles(src)) {
            File.SetAttributes(file, FileAttributes.None);
        }

        foreach (string folder in Directory.EnumerateDirectories(src)) {
            ClearAttributes(folder);
        }
    }

    public static void DeleteTargets(string src, string[] targets, bool recursive)
    {
        foreach (string target in targets) {
            string directory = Path.Combine(src, target);
            if (Directory.Exists(directory)) {
                Directory.Delete(directory, recursive);
            }
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
            else if (ext is INI_EXT) {
                // Some .ini files are exluded, but they
                // are specified by the entire file name
                ext = Path.GetFileName(file);
            }

            if (!excludeFiles.Contains(ext)) {
                Directory.CreateDirectory(dst);
                string dstFile = Path.Combine(dst, Path.GetFileName(file));
                File.Copy(file, dstFile, overwrite);
                File.SetAttributes(dstFile, FileAttributes.None);
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

    public static string? LocateTargets(string src, params string[] targets)
    {
        foreach (string folder in Directory.EnumerateDirectories(src)) {
            if (targets.Contains(Path.GetFileName(folder))) {
                return src;
            }

            return LocateTargets(folder, targets);
        }

        return null;
    }
}
