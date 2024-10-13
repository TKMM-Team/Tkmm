using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Core.IO;

namespace Tkmm.Core.Providers;

public class ModWriterProvider : IModWriterProvider
{
    public IModWriter GetSystemWriter(ModContext context)
    {
        return new SystemModWriter(context.Id);
    }
}