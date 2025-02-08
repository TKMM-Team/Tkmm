global using static TkSharp.Core.Common.TkLocalizationInterface;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using Tkmm.Core.IO.Readers;
using Tkmm.Core.Providers;
using Tkmm.Core.Services;
using TkSharp;
using TkSharp.Core;
using TkSharp.Core.Models;
using TkSharp.Extensions.GameBanana.Readers;
using TkSharp.Extensions.LibHac;
using TkSharp.IO.Writers;
using TkSharp.Merging;

namespace Tkmm.Core;

// ReSharper disable once InconsistentNaming
public static class TKMM
{
    private static readonly Lazy<TkExtensibleRomProvider> _romProvider = new(() => TkConfig.Shared.CreateRomProvider());
    private static readonly TkModReaderProvider _readerProvider;
    private static ITkThumbnailProvider? _thumbnailProvider;

#if SWITCH
    public static readonly string MergedOutputFolder = "/flash/atmosphere/contents/0100F2C0115B6000";
#else
    public static readonly string MergedOutputFolder = Path.Combine(AppContext.BaseDirectory, ".merged");
#endif


    public static ITkRom GetTkRom() => _romProvider.Value.GetRom();

    public static ITkRom? TryGetTkRom()
    {
        _romProvider.Value.TryGetRom(out ITkRom? rom);
        return rom;
    }

    public static Config Config => Config.Shared;

    public static TkModManager ModManager { get; }

    public static Task Initialize(ITkThumbnailProvider thumbnailProvider, CancellationToken ct = default)
    {
        _thumbnailProvider = thumbnailProvider;

        return Task.WhenAll(
            ModManager.Mods.Select(x => Task.Run(() => thumbnailProvider.ResolveThumbnail(x, ct), ct))
        );
    }

    public static async ValueTask<TkMod?> Install(object input, Stream? stream = null, TkModContext context = default, TkProfile? profile = null, CancellationToken ct = default)
    {
        if (_readerProvider.GetReader(input) is not ITkModReader reader) {
            TkLog.Instance.LogError("Could not locate mod reader for input: '{Input}'", input);
            return null;
        }

        if (await reader.ReadMod(input, stream, context, ct) is not TkMod mod) {
            TkLog.Instance.LogError("Failed to parse mod from input: '{Input}'", input);
            return null;
        }

        if (_thumbnailProvider?.ResolveThumbnail(mod, ct) is Task resolveThumbnail) {
            await resolveThumbnail;
        }

        ModManager.Import(mod, profile);
        return mod;
    }

    public static async ValueTask Merge(TkProfile profile, string? ipsOutputPath = null, CancellationToken ct = default)
    {
        DirectoryHelper.DeleteTargetsFromDirectory(MergedOutputFolder, ["romfs", "exefs", "cheats"], recursive: true);
        
        string metadataFilePath = Path.Combine(MergedOutputFolder, "romfs_metadata.bin");
        if (File.Exists(metadataFilePath)) {
            File.Delete(metadataFilePath);
        }

        FolderModWriter writer = new(MergedOutputFolder);
        
#if SWITCH
        // Since the FolderModWriter is writing to the merged output,
        // this path jumps back to write behind that folder.
        ipsOutputPath ??= Path.Combine("..", "..", "exefs_patches", "TKMM");
#endif

        using ITkRom tkRom = GetTkRom();
        TkMerger merger = new(writer, tkRom, Config.Shared.GameLanguage, ipsOutputPath);

        long startTime = Stopwatch.GetTimestamp();

        await merger.MergeAsync(GetMergeTargets(profile), ct);
        TkOptimizerService.Context.Apply(writer, profile);

        TimeSpan delta = Stopwatch.GetElapsedTime(startTime);
        TkLog.Instance.LogInformation("Elapsed time: {TotalMilliseconds}ms", delta.TotalMilliseconds);
    }

    /// <summary>
    /// Merges exefs, subsdk and cheats
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="ct"></param>
    public static void MergeBasic(TkProfile? profile = null, CancellationToken ct = default)
    {
        DirectoryHelper.DeleteTargetsFromDirectory(MergedOutputFolder, ["cheats", "exefs"],
            target => Path.GetExtension(target.AsSpan()) is not ".ips", recursive: true);
        
        ITkModWriter writer = new FolderModWriter(MergedOutputFolder);

        long startTime = Stopwatch.GetTimestamp();

        TkChangelog[] targets = GetMergeTargets(profile)
            .ToArray();

        TkMerger.MergeCheats(writer, targets);
        TkMerger.MergeExeFs(writer, targets);
        TkMerger.MergeSubSdk(writer, targets);

        TimeSpan delta = Stopwatch.GetElapsedTime(startTime);
        TkLog.Instance.LogInformation("Elapsed time: {TotalMilliseconds}ms", delta.TotalMilliseconds);
    }

    static TKMM()
    {
        ModManager = TkModManager.CreatePortable();
        ModManager.CurrentProfile = ModManager.GetCurrentProfile();

        _readerProvider = new TkModReaderProvider(ModManager, _romProvider.Value);
        _readerProvider.Register(new GameBananaModReader(_readerProvider));
        _readerProvider.Register(new External7zModReader(ModManager, _romProvider.Value));
    }

    private static IEnumerable<TkChangelog> GetMergeTargets(TkProfile? profile = null)
    {
        profile ??= ModManager.GetCurrentProfile();
        IEnumerable<TkChangelog> targets = TkModManager.GetMergeTargets(profile);
        
        if (TkOptimizerService.GetMod(profile) is { } optimizer) {
            return targets.Append(optimizer);
        }
        
        return targets;
    }
}