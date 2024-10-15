using Microsoft.Extensions.Logging;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Common;
using Tkmm.Common.Providers;
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
    
    public static ITkShopManager ShopManager { get; } = new TkShopManager();

    public static IMergerProvider MergerProvider { get; } = new TkMergerProvider();

    public static TkMergerMarshal MergerMarshal { get; } = new(ModManager, MergerProvider, Romfs);

    public static IChangelogBuilderProvider ChangelogBuilderProvider { get; } = new TkChangelogBuilderProvider();

    public static TkChangelogBuilderMarshal ChangelogBuilderMarshal { get; } = new(ChangelogBuilderProvider);
}