using Tkmm.Abstractions.IO;

namespace Tkmm.Core.IO.ModSources;

public sealed class FolderModSource(string sourceFolder) : IModSource
{
    private readonly string _optionsPath = Path.Combine(sourceFolder, ".options");

    public string RomfsPath { get; } = Path.Combine(sourceFolder, "romfs");

    public IEnumerable<string> RomfsFiles
        => Directory.EnumerateFiles(RomfsPath, "*.*", SearchOption.AllDirectories);

    public IEnumerable<IModChangelogSource> OptionSources
        => Directory.Exists(_optionsPath)
            ? Directory.EnumerateDirectories(_optionsPath, "*.*", SearchOption.AllDirectories)
                .SelectMany(optionGroupFolder => Directory.EnumerateDirectories(optionGroupFolder, "*.*", SearchOption.AllDirectories))
                .Select(optionFolder => new FolderModSource(optionFolder))
            : [];

    public ValueTask<(Stream Stream, long StreamLength)> OpenRead(string file, CancellationToken ct = default)
    {
        Stream fs = File.OpenRead(file);

        return ValueTask.FromResult(
            (fs, fs.Length)
        );
    }

    public ValueTask<bool> IsKnownInput<T>(T? input) where T : class
    {
        return ValueTask.FromResult(
            input is string directory && Directory.Exists(directory)
        );
    }
}