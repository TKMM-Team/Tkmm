using Tkmm.Abstractions.IO;

namespace Tkmm.Abstractions.Providers;

public interface IRomfsProvider
{
    IRomfs GetRomfs();
}