using Tkmm.Abstractions.IO;

namespace Tkmm.Core.IO.ModWriters;

public class FolderModWriter(string folderPath) : IModWriter
{
    private readonly string _folderPath = folderPath;
    
    public ValueTask<Stream> OpenWrite(string manifestFileName)
    {
        string filePath = Path.Combine(_folderPath, manifestFileName);
        return ValueTask.FromResult<Stream>(
            File.Create(filePath)
        );
    }
}