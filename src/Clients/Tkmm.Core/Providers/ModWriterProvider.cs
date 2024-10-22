using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Core.IO.ModWriters;

namespace Tkmm.Core.Providers;

public class ModWriterProvider : IModWriterProvider
{
    public static readonly ModWriterProvider Instance = new();
    
    public IModWriter GetSystemWriter(ModContext context)
    {
        return new SystemModWriter(context.Id);
    }
}