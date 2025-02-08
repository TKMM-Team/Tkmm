namespace Tkmm.Core.Helpers;

public static class DirectoryHelper
{
    public static void DeleteTargetsFromDirectory(string targetDirectory, string[] targets, bool recursive = false)
    {
        foreach (string target in targets) {
            string absolutePath = Path.Combine(targetDirectory, target);
            DeleteTarget(absolutePath, recursive);
        }
    }
    
    private static void DeleteTarget(string target, bool recursive = false)
    {
        if (File.Exists(target)) {
            File.Delete(target);
            return;
        }
        
        if (Directory.Exists(target)) {
            Directory.Delete(target, recursive);
        }
    }
    
    public static void DeleteTargetsFromDirectory(string targetDirectory, string[] targets, Func<string, bool> filter, bool recursive = false)
    {
        foreach (string target in targets) {
            string absolutePath = Path.Combine(targetDirectory, target);
            DeleteTarget(absolutePath, filter, recursive);
        }
    }
    
    private static void DeleteTarget(string target, Func<string, bool> filter, bool recursive = false)
    {
        if (File.Exists(target) && filter(target)) {
            File.Delete(target);
            return;
        }
        
        if (Directory.Exists(target)) {
            if (DeleteTargetFolder(target, filter, recursive)) {
                Directory.Delete(target);
            }
        }
    }
    
    /// <summary>
    /// Returns true if the <paramref name="target"/> can be deleted.
    /// </summary>
    private static bool DeleteTargetFolder(string target, Func<string, bool> filter, bool recursive = false)
    {
        int deleted = 0;
        string[] files = Directory.GetFiles(target);
        
        foreach (string file in files.Where(filter)) {
            deleted++;
            File.Delete(file);
        }
        
        string[] folders = Directory.GetDirectories(target);
        
        if (!recursive) {
            return folders.Length + files.Length == deleted;
        }

        foreach (string folder in folders.Where(folder => filter(folder) && DeleteTargetFolder(folder, filter, recursive))) {
            deleted++;
            Directory.Delete(folder);
        }
        
        return folders.Length + files.Length == deleted;
    }

    public static void Copy(string source, string output, bool overwrite = false)
    {
        Directory.CreateDirectory(output);
        
        foreach (string sourceFile in Directory.EnumerateFiles(source)) {
            string outputFile = Path.Combine(output, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, outputFile, overwrite);
        }

        foreach (string directory in Directory.EnumerateDirectories(source)) {
            string outputDirectory = Path.Combine(output, Path.GetFileName(directory));
            Copy(directory, outputDirectory, overwrite);
        }
    }
}