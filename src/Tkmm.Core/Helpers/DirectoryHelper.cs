using System.Runtime.Versioning;

namespace Tkmm.Core.Helpers;

public static class DirectoryHelper
{
    public static void DeleteTargetsFromDirectory(string targetDirectory, string[] targets, bool recursive = false)
    {
        foreach (var target in targets) {
            var absolutePath = Path.Combine(targetDirectory, target);
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
        foreach (var target in targets) {
            var absolutePath = Path.Combine(targetDirectory, target);
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
        var deleted = 0;
        var files = Directory.GetFiles(target);
        
        foreach (var file in files.Where(filter)) {
            deleted++;
            File.Delete(file);
        }
        
        var folders = Directory.GetDirectories(target);
        
        if (!recursive) {
            return folders.Length + files.Length == deleted;
        }

        foreach (var folder in folders.Where(folder => filter(folder) && DeleteTargetFolder(folder, filter, recursive))) {
            deleted++;
            Directory.Delete(folder);
        }
        
        return folders.Length + files.Length == deleted;
    }

    public static void Copy(string source, string output, bool overwrite = false)
    {
        source = Path.GetFullPath(source);
        output = Path.GetFullPath(output);

        if (output.Length >= source.Length && output[..source.Length] == source) {
            throw new InvalidOperationException(
                $"The folder '{source}' cannot be recursively copied into itself ('{output}').");
        }
        
        Directory.CreateDirectory(output);
        
        foreach (var sourceFile in Directory.EnumerateFiles(source)) {
            var outputFile = Path.Combine(output, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, outputFile, overwrite);
        }

        foreach (var directory in Directory.EnumerateDirectories(source)) {
            var outputDirectory = Path.Combine(output, Path.GetFileName(directory));
            Copy(directory, outputDirectory, overwrite);
        }
    }

    public static void HideTargetsInDirectory(string directory, params Span<string> targets)
    {
        foreach (var target in targets) {
            var path = Path.Combine(directory, target);
            
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            
            DirectoryInfo info = new(path);
            info.Attributes |= FileAttributes.Hidden;
        }
    }
}