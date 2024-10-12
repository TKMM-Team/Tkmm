namespace Tkmm.Abstractions.IO;

public interface IModWriter
{
    ValueTask CreateFile(string manifestFileName, Span<byte> contents);
}