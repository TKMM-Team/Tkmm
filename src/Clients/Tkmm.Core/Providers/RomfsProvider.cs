using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Core.IO;

namespace Tkmm.Core.Providers;

public class RomfsProvider : IRomfsProvider
{
    public IRomfs GetRomfs()
    {
        // TODO: Return romfs based on user config (Xci, Nsp, Extracted, etc)
        return ExtractedRomfs.Instance;
    }
}