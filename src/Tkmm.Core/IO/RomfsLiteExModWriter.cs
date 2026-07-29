using TkSharp.Core;

namespace Tkmm.Core.IO;

/// <summary>
/// Remaps <c>romfs/...</c> writes into <c>RomfsLiteEX/TKMM{n}/...</c>,
/// rotating buckets every <see cref="MAX_FILES_PER_BUCKET"/> files for FAT32 limits.
/// </summary>
public sealed class RomfsLiteExModWriter(ITkModWriter inner) : ITkModWriter
{
    private const int MAX_FILES_PER_BUCKET = 3000;
    private const string ROOT_FOLDER_NAME = "RomfsLiteEX";

    private readonly HashSet<string> _writtenInCurrentBucket = new(StringComparer.OrdinalIgnoreCase);

    private int _bucketIndex = 1;

    public Stream OpenWrite(string filePath)
    {
        var normalized = filePath.Replace('\\', '/');

        if (!normalized.StartsWith("romfs/", StringComparison.OrdinalIgnoreCase)) {
            return inner.OpenWrite(filePath);
        }

        var relative = normalized["romfs/".Length..];
        if (_writtenInCurrentBucket.Count >= MAX_FILES_PER_BUCKET
            && !_writtenInCurrentBucket.Contains(relative)) {
            _bucketIndex++;
            _writtenInCurrentBucket.Clear();
        }

        _writtenInCurrentBucket.Add(relative);
        var remapped = Path.Combine(ROOT_FOLDER_NAME, $"TKMM{_bucketIndex}", relative.Replace('/', Path.DirectorySeparatorChar));
        return inner.OpenWrite(remapped);
    }

    public void SetRelativeFolder(string rootFolder)
        => inner.SetRelativeFolder(rootFolder);
}
