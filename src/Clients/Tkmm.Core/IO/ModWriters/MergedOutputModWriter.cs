using Tkmm.Abstractions.IO;

namespace Tkmm.Core.IO.ModWriters;

public sealed class MergedOutputModWriter : IModWriter
{
    public static readonly MergedOutputModWriter Instance = new();
    
    public ValueTask<Stream> OpenWrite(string manifestFileName)
    {
        string targetOutputFile = Path.Combine(ModManager.MergedModsFolder, manifestFileName);

        return ValueTask.FromResult<Stream>(
            File.OpenWrite(targetOutputFile)
        );
    }
}