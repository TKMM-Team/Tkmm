using System.IO.Compression;
using Tkmm.Abstractions.IO;

namespace Tkmm.Core.IO.ModWriters;

public class TkclModWriter(string tkclFilePath) : IModWriter, IDisposable
{
    private readonly ZipArchive _tkcl = ZipFile.Open(tkclFilePath, ZipArchiveMode.Create);

    public ValueTask<Stream> OpenWrite(string manifestFileName)
    {
        return ValueTask.FromResult(
            _tkcl.CreateEntry(manifestFileName).Open());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _tkcl.Dispose();
    }
}