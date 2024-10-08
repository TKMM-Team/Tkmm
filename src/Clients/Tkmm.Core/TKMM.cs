using Microsoft.Extensions.Logging;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Core.IO;
using Tkmm.Core.Providers;

namespace Tkmm.Core;

// ReSharper disable once InconsistentNaming

public static class TKMM
{
    private static readonly Lazy<ILogger> _logger = new(() => new DesktopLogger());
    private static readonly Lazy<IRomfsProvider> _romfsProvider = new(() => new RomfsProvider());
    
    public static ILogger Logger => _logger.Value;

    public static IRomfs Romfs => _romfsProvider.Value.GetRomfs();
    
    public static Config Config => Config.Shared;
    
    public static IModManager ModManager { get; } = new ModManager();
}