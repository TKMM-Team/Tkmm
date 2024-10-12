namespace Tkmm.Abstractions.IO;

public interface IModWriter
{
    ValueTask<Stream> OpenWrite(string manifestFileName);
}