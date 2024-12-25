using TkSharp.Core;

namespace Tkmm.Core.Providers;

public class TkRomProvider : ITkRomProvider
{
    public ITkRom GetRom()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Logs the current status of the configured rom.
    /// </summary>
    public static void LogRomConfigInfo()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns true if a rom can be provided with the current configurations. 
    /// </summary>
    public static bool CanProvideRom(out string? reason)
    {
        throw new NotImplementedException();
    }
}