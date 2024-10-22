using System.Runtime.CompilerServices;
using Tkmm.Abstractions.IO;

namespace Tkmm.Core.IO.ModSources;

public sealed class FolderModSource(string sourceFolder) : ModSource<string>(Path.Combine(sourceFolder, "romfs"))
{
    protected override IEnumerable<string> RomfsFiles
        => Directory.EnumerateFiles(RomfsPath, "*.*", SearchOption.AllDirectories);

    protected override ValueTask<(Stream Stream, long StreamLength)> OpenRead(string input, CancellationToken ct = default)
    {
        Stream fs = File.OpenRead(input);

        return ValueTask.FromResult(
            (fs, fs.Length)
        );
    }

    public override ValueTask<bool> IsKnownInput<TSource>(TSource? input) where TSource : class
    {
        return ValueTask.FromResult(
            input is string directory && Directory.Exists(directory)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override string GetFileName(string input)
    {
        return input;
    }
}