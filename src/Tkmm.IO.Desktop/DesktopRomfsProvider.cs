using Ninject;
using Tkmm.Core;
using Tkmm.Core.Abstractions.IO;

namespace Tkmm.IO.Desktop;

public class DesktopRomfsProvider : IRomfsProvider
{
    public IRomfs GetRomfs()
    {
        return TKMM.DI.Get<ExtractedRomfs>();
    }
}