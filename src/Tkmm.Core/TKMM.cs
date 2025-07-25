global using static TkSharp.Core.Common.TkLocalizationInterface;
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using Tkmm.Core.IO.Readers;
using Tkmm.Core.Providers;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer;
using TkSharp;
using TkSharp.Core;
using TkSharp.Core.Extensions;
using TkSharp.Core.Models;
using TkSharp.Extensions.GameBanana.Readers;
using TkSharp.Extensions.LibHac;
using TkSharp.IO.Writers;
using TkSharp.Merging;
using TkSharp.Packaging.IO.Serialization;

namespace Tkmm.Core;

// ReSharper disable once InconsistentNaming
public static class TKMM
{
#if READONLY_FS
    public static readonly string BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tkmm2");
#else
    public static readonly string BaseDirectory = AppContext.BaseDirectory;
#endif
    
    private static readonly TkModReaderProvider _readerProvider;
    private static ITkThumbnailProvider? _thumbnailProvider;

    private static TkExtensibleRomProvider RomProvider => TkConfig.Shared.CreateRomProvider();
    
#if SWITCH
    public static readonly string MergedOutputFolder = "/flash/atmosphere/contents/0100F2C0115B6000";
#else
    public static string MergedOutputFolder => Config.Shared.MergeOutput ?? Path.Combine(BaseDirectory, "Merged");
#endif

    public static ITkRom GetTkRom() => RomProvider.GetRom();

    public static ITkRom? TryGetTkRom()
        => TryGetTkRom(out _, out _, out _);

    public static ITkRom? TryGetTkRom(out string? error)
        => TryGetTkRom(out _, out _, out error);

    public static ITkRom? TryGetTkRom(out bool hasBaseGame, out bool hasUpdate)
        => TryGetTkRom(out hasBaseGame, out hasUpdate, out _);

    public static ITkRom? TryGetTkRom(out bool hasBaseGame, out bool hasUpdate, out string? error)
        => RomProvider.TryGetRom(out hasBaseGame, out hasUpdate, out error);

    public static Config Config => Config.Shared;

    public static TkModManager ModManager { get; }

    public static Task Initialize(ITkThumbnailProvider thumbnailProvider, CancellationToken ct = default)
    {
        _thumbnailProvider = thumbnailProvider;

        return Task.WhenAll(
            ModManager.Mods.Select(x => Task.Run(() => thumbnailProvider.ResolveThumbnail(x, ct), ct))
        );
    }

    public static async ValueTask<TkMod?> Install(object input, Stream? stream = null, TkModContext? context = null, TkProfile? profile = null, CancellationToken ct = default)
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

    public static async ValueTask Merge(TkProfile profile, string? ipsOutputPath = null, string? mergeOutput = null, CancellationToken ct = default)
    {
        mergeOutput ??= MergedOutputFolder;
        
        DirectoryHelper.DeleteTargetsFromDirectory(mergeOutput, ["romfs", "exefs", "cheats"], recursive: true);

        string metadataFilePath = Path.Combine(mergeOutput, "romfs_metadata.bin");
        if (File.Exists(metadataFilePath)) {
            File.Delete(metadataFilePath);
        }

        FolderModWriter writer = new(mergeOutput);

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
        TkChangelogBuilder.Init(RomProvider);
        
        string legacyDataFolder = Path.Combine(BaseDirectory, ".data");
        string dataFolder = Path.Combine(BaseDirectory, ".data2");
        
        if (Directory.Exists(legacyDataFolder)) {
            try {
                if (!TkSystemUpdater.UpdateToData2(dataFolder, legacyDataFolder)) {
                    goto SkipLegacyUpdate;
                }
                
                try {
                    Directory.Delete(legacyDataFolder, true);
                }
                catch (Exception ex) {
                    TkLog.Instance.LogError(ex, "Could not delete legacy data folder");
                }
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, "Failed to upgrade legacy data folder");
            }
        } 
        
    SkipLegacyUpdate:
        ModManager = TkModManager.Create(dataFolder);
        ModManager.CurrentProfile = ModManager.GetCurrentProfile();

        ModManager.PropertyChanged += static (s, e) => {
            if (e.PropertyName == nameof(TkModManager.CurrentProfile)) {
                MergeBasic();
                TkOptimizerService.Context.ApplyToMergedOutput();
            }
        };

        _readerProvider = new TkModReaderProvider(ModManager, RomProvider);
        _readerProvider.Register(new GameBananaModReader(_readerProvider));
        _readerProvider.Register(new External7zModReader(ModManager, RomProvider));

        Span<string> hiddenSystemFolders = [".data", ".data2", ".layout"];
        DirectoryHelper.HideTargetsInDirectory(BaseDirectory, hiddenSystemFolders);

        if (Environment.ProcessPath is null) {
            return;
        }
        
        const string processName = "Tkmm";
        RegistryHelper.CreateGameBananaWebProtocol(processName, Environment.ProcessPath);
        RegistryHelper.CreateFileAssociations(processName, ".tkcl", Environment.ProcessPath);
    }

    private static IEnumerable<TkChangelog> GetMergeTargets(TkProfile? profile = null)
    {
        profile ??= ModManager.GetCurrentProfile();
        Ulid optimizerId = TkOptimizerService.GetStaticId();
        return TkModManager.GetMergeTargets(profile, mod => mod.Mod.Id != optimizerId)
            .Append(TkOptimizerService.GetMod(profile));
    }

    public static async ValueTask ExportPackage(TkMod target, Stream output)
    {
        await Task.Run(() => {
            string modPath = Path.Combine(ModManager.ModsFolderPath, target.Id.ToString());
            using MemoryStream contentArchiveOutput = new();
            ZipFile.CreateFromDirectory(modPath, contentArchiveOutput);
            TkPackWriter.Write(output, target, contentArchiveOutput.GetSpan());
        });
    }

    public static async ValueTask ExportRomfs(TkMod target, string outputFolderPath, CancellationToken ct = default)
    {
        TkProfile profile = new() {
            Mods = {
                new TkProfileMod(target)
            }
        };
        
        // Make sure the optimizer is disabled
        TkOptimizerStore.CreateStore(profile)
            .IsEnabled = false;

        await Merge(profile, mergeOutput: outputFolderPath, ct: ct);
        TkOptimizerStore.Remove(profile);
    }
}