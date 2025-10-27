using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Attributes;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models;
using TkSharp.Core;
using TkSharp.Core.IO.Caching;
using TkSharp.Data.Embedded;
using TkSharp.Extensions.LibHac;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Core;

public sealed partial class TkConfig : ConfigModule<TkConfig>
{
    private const string DEFAULT_GAME_VERSION = "Auto";

    [JsonIgnore]
    public override string LocalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Config.Shared.Name, "TkConfig.json");

    public TkConfig()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo is { Exists: true, Length: 0 }) {
            File.Delete(LocalPath);
        }
    }

    [ObservableProperty]
    [property: Config(
        Header = "TkConfig_PreferredGameVersion",
        Description = "TkConfig_PreferredGameVersionDescription",
        Group = "ConfigSection_GameDump")]
    [property: DropdownConfig(DEFAULT_GAME_VERSION, "1.4.2", "1.4.1", "1.4.0", "1.2.1", "1.2.0", "1.1.2", "1.1.1", "1.1.0")]
    private string _preferredGameVersion = DEFAULT_GAME_VERSION;

    [ObservableProperty]
    [property: Config(
        Header = "TkConfig_KeysFolderPath",
        Description = "TkConfig_KeysFolderPathDescription",
        Group = "ConfigSection_GameDump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "keys-folder-path",
        Title = "TkConfig_SelectKeysFolder")]
    private string? _keysFolderPath;

    [ObservableProperty]
    [property: Config(
        Header = "TkConfig_PackagedBaseGamePaths",
        Description = "TkConfig_PackagedBaseGamePathsDescription",
        Group = "ConfigSection_GameDump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "XCI/NSP:*.nsp;*.xci|All files:*.*",
        InstanceBrowserKey = "base-game-file-path",
        Title = "TkConfig_SelectBaseGame")]
    [property: PathCollectionOptions(PathType.FileOrFolder)]
    private PathCollection _packagedBaseGamePaths = [];

    [ObservableProperty]
    [property: Config(
        Header = "TkConfig_GameUpdateFilePaths",
        Description = "TkConfig_GameUpdateFilePathsDescription",
        Group = "ConfigSection_GameDump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "NSP:*.nsp|All files:*.*",
        InstanceBrowserKey = "game-update-file-path",
        Title = "TkConfig_SelectGameUpdate")]
    [property: PathCollectionOptions(PathType.File)]
    private PathCollection _packagedUpdatePaths = [];

    [ObservableProperty]
    [property: Config(
        Header = "TkConfig_SdCardRootPath",
        Description = "TkConfig_SdCardRootPathDescription",
        Group = "ConfigSection_GameDump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "sd-card-root-path",
        Title = "TkConfig_SelectSdCardRoot")]
    private string? _sdCardRootPath;

#if SWITCH
    [ObservableProperty]
    private PathCollection _gameDumpFolderPaths = [];
    [ObservableProperty]
    private PathCollection _nandFolderPaths = [];
#else
    [ObservableProperty]
    [property: Config(
        Header = "TkConfig_GameDumpFolderPaths",
        Description = "TkConfig_GameDumpFolderPathsDescription",
        Group = "ConfigSection_GameDump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "game-dump-folder-path",
        Title = "TkConfig_SelectGameDumpFolder")]
    [property: PathCollectionOptions(PathType.Folder)]
    private PathCollection _gameDumpFolderPaths = [];

    [ObservableProperty]
    [property: Config(
        Header = "TkConfig_NandFolderPaths",
        Description = "TkConfig_NandFolderPathsDescription",
        Group = "ConfigSection_GameDump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "nand-folder-path",
        Title = "TkConfig_SelectNandFolder")]
    [property: PathCollectionOptions(PathType.Folder)]
    private PathCollection _nandFolderPaths = [];
#endif

    public TkExtensibleRomProvider CreateRomProvider()
    {
        using var checksums = TkEmbeddedDataSource.GetChecksumsBin();
        using var packFileLookup = TkEmbeddedDataSource.GetPackFileLookup();

#if !SWITCH
        var builder = TkExtensibleRomProviderBuilder.Create(
                TkChecksums.FromStream(checksums), new TkPackFileLookup(packFileLookup)
            )
            .WithPreferredVersion(() => PreferredGameVersion is DEFAULT_GAME_VERSION ? null : PreferredGameVersion)
            .WithKeysFolder(() => KeysFolderPath)
            .WithExtractedGameDump(() => GameDumpFolderPaths)
            .WithPackagedBaseGame(() => PackagedBaseGamePaths);

        var emulatorFilePath = Config.Shared.EmulatorPath;
        if (string.IsNullOrWhiteSpace(emulatorFilePath) || !PreferredGameVersion.Equals(DEFAULT_GAME_VERSION)) {
            goto Configured;
        }

        var exeName = Path.GetFileName(emulatorFilePath);

        if (Path.GetFileNameWithoutExtension(exeName).Equals("ryujinx", StringComparison.InvariantCultureIgnoreCase)) {
            try {
                if (TkRyujinxHelper.GetSelectedUpdatePath(emulatorFilePath) is { } updateFilePath) {
                    return builder
                        .WithSdCard(() => null)
                        .WithPackagedUpdate(() => [updateFilePath])
                        .WithNand(() => null)
                        .Build();
                }

                throw new Exception(Locale["TkConfig_ErrorNoUpdateSelected"]);
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, Locale["TkConfig_ErrorFailedToDetectUpdate"]);
                goto Configured;
            }
        }

        try {
            if (TkEmulatorHelper.GetNandPath(emulatorFilePath) is { } emulatorNandPath && Directory.Exists(emulatorNandPath)) {
                var keys = TkKeyUtils.GetKeysFromFolder(KeysFolderPath!);
                if (keys == null) {
                    throw new Exception(Locale["TkConfig_ErrorKeysNotFound"]);
                }

                TkNandUtils.IsValid(keys, emulatorNandPath, out var hasUpdate);

                if (!hasUpdate) {
                    throw new Exception(Locale["TkConfig_ErrorNoUpdateOnNand"]);
                }

                return builder
                    .WithSdCard(() => null)
                    .WithPackagedUpdate(() => null)
                    .WithNand(() => [emulatorNandPath])
                    .Build();
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, Locale["TkConfig_ErrorEnsureKeysAndUpdate"]);
        }

    Configured:
         return builder
            .WithSdCard(() => SdCardRootPath)
            .WithPackagedUpdate(() => PackagedUpdatePaths)
            .WithNand(() => NandFolderPaths)
            .Build();
#else
        return TkExtensibleRomProviderBuilder.Create(TkChecksums.FromStream(checksums), new TkPackFileLookup(packFileLookup))
            .WithPreferredVersion(() => PreferredGameVersion is DEFAULT_GAME_VERSION ? null : PreferredGameVersion)
            .WithKeysFolder(() => KeysFolderPath)
            .WithSdCard(() => SdCardRootPath)
            .WithPackagedBaseGame(() => PackagedBaseGamePaths)
            .WithPackagedUpdate(() => PackagedUpdatePaths)
            .Build();
#endif
    }

    public override void Load(ref TkConfig module)
    {
        try {
           base.Load(ref module);
        }
        catch (Exception ex) {
            module = new TkConfig();
            TkLog.Instance.LogError(ex, string.Format(Locale["Config_ErrorFailedToLoadConfig"], nameof(TkConfig)));
        }
    }

    public override string Translate(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? input : Locale[input];
    }
}