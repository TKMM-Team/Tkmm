namespace Tkmm.Core.Helpers;

public static class DirectoryHelper
{

    private static readonly EnumerationOptions NoFollowReparsePoints = new() {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        AttributesToSkip = FileAttributes.ReparsePoint
    };

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
        Copy(source, output, overwrite, progress: null);
    }

    public static void Copy(string source, string output, bool overwrite, IProgress<(int Copied, int Total)>? progress)
    {
        source = Path.GetFullPath(source);
        output = Path.GetFullPath(output);

        if (IsSubPathOf(output, source)) {
            throw new InvalidOperationException(
                $"The folder '{source}' cannot be recursively copied into itself ('{output}').");
        }

        CopyFiles(
            Directory.EnumerateFiles(source, "*", NoFollowReparsePoints),
            source,
            output,
            overwrite,
            progress);
    }

    public static IReadOnlyList<string> GetMergeExportFiles(string source, bool useRomfsLite)
    {
        source = Path.GetFullPath(source);

        return GetMergeExportEntries(source, useRomfsLite)
            .SelectMany(entry => {
                var path = Path.Combine(source, entry);
                if (File.Exists(path)) {
                    return (IEnumerable<string>)[path];
                }

                return Directory.Exists(path)
                    ? Directory.EnumerateFiles(path, "*", NoFollowReparsePoints)
                    : [];
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static void CopyMergeOutput(string source, string output, bool useRomfsLite, bool overwrite,
        IProgress<(int Copied, int Total)>? progress)
    {
        source = Path.GetFullPath(source);
        output = Path.GetFullPath(output);

        if (IsSubPathOf(output, source)) {
            throw new InvalidOperationException(
                $"The folder '{source}' cannot be recursively copied into itself ('{output}').");
        }
        
        Directory.CreateDirectory(output);

        CopyFiles(GetMergeExportFiles(source, useRomfsLite), source, output, overwrite, progress);
    }

    public static IReadOnlyList<string> GetMergeExportEntries(string mergeOutput, bool useRomfsLite)
    {
        List<string> entries = [];

        var romfsRoot = ResolveRomfsRoot(mergeOutput, useRomfsLite);
        if (romfsRoot is not null) {
            entries.Add(romfsRoot);
        }

        foreach (var entry in (string[])["exefs", "cheats", "romfs_metadata.bin"]) {
            var path = Path.Combine(mergeOutput, entry);
            if (Directory.Exists(path) || File.Exists(path)) {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private static string? ResolveRomfsRoot(string mergeOutput, bool useRomfsLite)
    {
        string[] preferred = useRomfsLite
            ? ["RomfsLiteEX", "romfslite", "romfs"]
            : ["romfs", "romfslite", "RomfsLiteEX"];

        foreach (var name in preferred) {
            if (Directory.Exists(Path.Combine(mergeOutput, name))) {
                return name;
            }
        }

        return null;
    }

    private static void CopyFiles(IEnumerable<string> sourceFiles, string sourceRoot, string outputRoot, bool overwrite,
        IProgress<(int Copied, int Total)>? progress)
    {
        var files = sourceFiles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var total = files.Length;
        var copied = 0;

        Directory.CreateDirectory(outputRoot);

        foreach (var directory in files
                     .Select(file => Path.GetDirectoryName(Path.GetRelativePath(sourceRoot, file)))
                     .Where(relative => !string.IsNullOrEmpty(relative))
                     .Distinct(StringComparer.OrdinalIgnoreCase)) {
            Directory.CreateDirectory(Path.Combine(outputRoot, directory!));
        }

        foreach (var sourceFile in files) {
            var outputFile = Path.Combine(outputRoot, Path.GetRelativePath(sourceRoot, sourceFile));
            var outputDirectory = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(outputDirectory)) {
                Directory.CreateDirectory(outputDirectory);
            }

            File.Copy(sourceFile, outputFile, overwrite);
            copied++;
            progress?.Report((copied, total));
        }

        if (total == 0) {
            progress?.Report((0, 0));
        }
    }

    private static bool IsSubPathOf(string path, string putativeParent)
    {
        var relative = Path.GetRelativePath(putativeParent, path);
        return relative == "."
               || !relative.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relative);
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