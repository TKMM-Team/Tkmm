using Ninject;
using Tkmm.Core;
using Tkmm.Core.Abstractions.IO;

namespace Tkmm.Desktop.IO;

public class DesktopRomfsProvider : IRomfsProvider
{
    public IRomfs GetRomfs()
    {
        return TKMM.DI.Get<ExtractedRomfs>();
    }
}