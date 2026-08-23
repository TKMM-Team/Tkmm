using System.Collections.ObjectModel;
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
    public const string DefaultGameVersion = "Auto";

    private bool _isRefreshingVersions;
    private bool _dumpHandlersAttached;
    private bool _versionRefreshSuspended;

    [JsonIgnore]
    public override string LocalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Config.DATA_FOLDER_NAME, "TkConfig.json");

    [JsonIgnore]
    public IReadOnlyList<string> AvailableUpdateVersions { get; private set; } = [];

    [JsonIgnore]
    private ObservableCollection<string> PreferredGameVersionOptions { get; } = [];

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
    [property: DropdownConfig(RuntimeItemsSourceMethodName = nameof(GetPreferredGameVersionOptions))]
    private string _preferredGameVersion = DefaultGameVersion;

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

    public ObservableCollection<string> GetPreferredGameVersionOptions() => PreferredGameVersionOptions;

    public void ResetGameDumpSettings()
    {
        SuspendVersionRefresh();
        try {
            PreferredGameVersion = DefaultGameVersion;
            KeysFolderPath = null;
            PackagedBaseGamePaths = [];
            PackagedUpdatePaths = [];
            SdCardRootPath = null;
            GameDumpFolderPaths = [];
            NandFolderPaths = [];
        }
        finally {
            ResumeVersionRefresh();
        }
    }

    public void SuspendVersionRefresh() => _versionRefreshSuspended = true;

    public void ResumeVersionRefresh()
    {
        if (!_versionRefreshSuspended) {
            return;
        }

        _versionRefreshSuspended = false;
        RefreshAvailableUpdateVersions();
    }

    public void RefreshAvailableUpdateVersions()
    {
        if (_isRefreshingVersions || _versionRefreshSuspended) {
            return;
        }

        _isRefreshingVersions = true;
        try {
            using var checksums = TkEmbeddedDataSource.GetChecksumsBin();
            using var packFileLookup = TkEmbeddedDataSource.GetPackFileLookup();

            var builder = TkExtensibleRomProviderBuilder.Create(
                    TkChecksums.FromStream(checksums), new TkPackFileLookup(packFileLookup)
                )
                .WithKeysFolder(() => KeysFolderPath)
                .WithExtractedGameDump(() => GameDumpFolderPaths)
                .WithPackagedBaseGame(() => PackagedBaseGamePaths)
                .WithSdCard(() => SdCardRootPath)
                .WithPackagedUpdate(() => PackagedUpdatePaths)
                .WithNand(() => NandFolderPaths);

            AvailableUpdateVersions = builder.Build().GetAvailableUpdateVersions();

            PreferredGameVersionOptions.Clear();
#if !SWITCH
            if (!string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath)) {
                PreferredGameVersionOptions.Add(DefaultGameVersion);
            }
#endif
            foreach (var version in AvailableUpdateVersions) {
                PreferredGameVersionOptions.Add(version);
            }

            if (PreferredGameVersionOptions.Count > 0 && !PreferredGameVersionOptions.Contains(PreferredGameVersion)) {
                PreferredGameVersion = PreferredGameVersionOptions[0];
            }
        }
        finally {
            _isRefreshingVersions = false;
        }
    }

    public TkExtensibleRomProvider CreateRomProvider()
    {
        using var checksums = TkEmbeddedDataSource.GetChecksumsBin();
        using var packFileLookup = TkEmbeddedDataSource.GetPackFileLookup();

        var builder = TkExtensibleRomProviderBuilder.Create(
                TkChecksums.FromStream(checksums), new TkPackFileLookup(packFileLookup)
            )
            .WithPreferredVersion(() => PreferredGameVersion is DefaultGameVersion ? null : PreferredGameVersion)
            .WithKeysFolder(() => KeysFolderPath)
            .WithExtractedGameDump(() => GameDumpFolderPaths)
            .WithPackagedBaseGame(() => PackagedBaseGamePaths);

#if !SWITCH
        var emulatorFilePath = Config.Shared.EmulatorPath;
        if (string.IsNullOrWhiteSpace(emulatorFilePath) || !PreferredGameVersion.Equals(DefaultGameVersion)) {
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
#endif

    Configured:
         return builder
            .WithSdCard(() => SdCardRootPath)
            .WithPackagedUpdate(() => PackagedUpdatePaths)
            .WithNand(() => NandFolderPaths)
            .Build();
    }

    partial void OnKeysFolderPathChanged(string? value) => RefreshAvailableUpdateVersions();
    partial void OnSdCardRootPathChanged(string? value) => RefreshAvailableUpdateVersions();

    partial void OnPackagedBaseGamePathsChanged(PathCollection value) => AttachPathCollection(value);
    partial void OnPackagedUpdatePathsChanged(PathCollection value) => AttachPathCollection(value);
    partial void OnGameDumpFolderPathsChanged(PathCollection value) => AttachPathCollection(value);
    partial void OnNandFolderPathsChanged(PathCollection value) => AttachPathCollection(value);

    private void AttachDumpPathHandlers()
    {
        if (_dumpHandlersAttached) {
            return;
        }

        AttachPathCollection(PackagedBaseGamePaths);
        AttachPathCollection(PackagedUpdatePaths);
        AttachPathCollection(GameDumpFolderPaths);
        AttachPathCollection(NandFolderPaths);
        _dumpHandlersAttached = true;
        RefreshAvailableUpdateVersions();
    }

    private void AttachPathCollection(PathCollection paths)
    {
        paths.PathsChanged -= OnDumpPathsChanged;
        paths.PathsChanged += OnDumpPathsChanged;

        if (_dumpHandlersAttached) {
            RefreshAvailableUpdateVersions();
        }
    }

    private void OnDumpPathsChanged(object? sender, EventArgs e) => RefreshAvailableUpdateVersions();

    public override void Load(ref TkConfig module)
    {
        try {
           base.Load(ref module);
        }
        catch (Exception ex) {
            module = new TkConfig();
            TkLog.Instance.LogError(ex, string.Format(Locale["Config_ErrorFailedToLoadConfig"], nameof(TkConfig)));
        }

        module.AttachDumpPathHandlers();
    }

    public override string Translate(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? input : Locale[input];
    }
}
