using Microsoft.Extensions.Logging;
using Ninject;
using Tkmm.Core.Abstractions;
using Tkmm.Core.Abstractions.IO;

namespace Tkmm.Core;

// ReSharper disable UnusedType.Global, InconsistentNaming
public static class TKMM
{
    private static readonly Lazy<ILogger> _logger = new(() =>
        DI.Get<ILogger>()
    );
    
    private static readonly Lazy<IRomfsProvider> _romfsProvider = new(() =>
        DI.Get<IRomfsProvider>()
    );

    public static Config Config => Config.Shared;

    public static IKernel DI { get; } = new StandardKernel();

    public static ILogger Logger => _logger.Value;

    public static IRomfs Romfs => _romfsProvider.Value.GetRomfs();

    public static ITkModManager ModManager => DI.Get<ITkModManager>();
}