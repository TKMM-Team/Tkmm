using Tkmm.Abstractions.IO;

namespace Tkmm.Core.IO.ModWriters;

public sealed class SystemModWriter(Ulid id) : IModWriter
{
    private readonly string _id = id.ToString();

    public ValueTask<Stream> OpenWrite(string manifestFileName)
    {
        string targetOutputFile = Path.Combine(ModManager.SystemModsFolder, _id, manifestFileName);
        
        return ValueTask.FromResult<Stream>(
            File.OpenWrite(targetOutputFile)
        );
    }
}