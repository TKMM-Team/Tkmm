using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using LibHac.Common.Keys;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Attributes;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models;
using TkSharp.Core;
using TkSharp.Data.Embedded;
using TkSharp.Extensions.LibHac;
using TkSharp.Extensions.LibHac.Util;

namespace Tkmm.Core;

public sealed partial class TkConfig : ConfigModule<TkConfig>
{
    public const string DEFAULT_GAME_VERSION = "Auto";

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
        Header = "Preferred Game Version",
        Description = "The game version to look for when reading the configured SD card if multiple versions are found.",
        Group = "Game Dump")]
    [property: DropdownConfig(DEFAULT_GAME_VERSION, "1.2.1", "1.2.0", "1.1.2", "1.1.1", "1.1.0")]
    private string _preferredGameVersion = DEFAULT_GAME_VERSION;

    [ObservableProperty]
    [property: Config(
        Header = "Keys Folder Path",
        Description = "The absolute path to the folder containing your dumped Switch keys.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "keys-folder-path",
        Title = "Select prod/title keys folder")]
    private string? _keysFolderPath;

    [ObservableProperty]
    [property: Config(
        Header = "Packaged Base Game Path(s)",
        Description = "The absolute path to your dumped base game file (.xci or .nsp) or split folder.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "XCI/NSP:*.nsp;*.xci|All files:*.*",
        InstanceBrowserKey = "base-game-file-path",
        Title = "Select base game XCI/NSP")]
    [property: PathCollectionOptions(PathType.FileOrFolder)]
    private PathCollection _packagedBaseGamePaths = [];

    [ObservableProperty]
    [property: Config(
        Header = "Game Update File Path(s)",
        Description = "The absolute path to your dumped game update file (.nsp) or split folder.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "NSP:*.nsp|All files:*.*",
        InstanceBrowserKey = "game-update-file-path",
        Title = "Select game update NSP")]
    [property: PathCollectionOptions(PathType.FileOrFolder)]
    private PathCollection _packagedUpdatePaths = [];

    [ObservableProperty]
    [property: Config(
        Header = "SD Card Root Path",
        Description = "The path to the root of your SD card or emuMMC (must contain a Nintendo/Contents folder with the base game or update installed).",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "sd-card-root-path",
        Title = "Select SD card root folder")]
    private string? _sdCardRootPath;

#if SWITCH
    [ObservableProperty]
    private PathCollection _gameDumpFolderPaths = [];
    [ObservableProperty]
    private PathCollection _nandFolderPaths = [];
#else
    [ObservableProperty]
    [property: Config(
        Header = "Game Dump Folder Path(s)",
        Description = "The absolute path to your dumped RomFS folder.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "game-dump-folder-path",
        Title = "Select game dump folder path (with update 1.1.0 or later)")]
    [property: PathCollectionOptions(PathType.Folder)]
    private PathCollection _gameDumpFolderPaths = [];

    [ObservableProperty]
    [property: Config(
        Header = "NAND Folder Path(s)",
        Description = "The absolute path to your virtual NAND folder.",
        Group = "Game Dump")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "nand-folder-path",
        Title = "Select virtual NAND folder path")]
    [property: PathCollectionOptions(PathType.Folder)]
    private PathCollection _nandFolderPaths = [];
#endif
    
    public TkExtensibleRomProvider CreateRomProvider()
    {
        using Stream checksums = TkEmbeddedDataSource.GetChecksumsBin();

#if !SWITCH
        TkExtensibleRomProviderBuilder builder = TkExtensibleRomProviderBuilder.Create(checksums)
            .WithPreferredVersion(() => PreferredGameVersion is DEFAULT_GAME_VERSION ? null : PreferredGameVersion)
            .WithKeysFolder(() => KeysFolderPath)
            .WithExtractedGameDump(() => GameDumpFolderPaths)
            .WithPackagedBaseGame(() => PackagedBaseGamePaths);

        string? emulatorFilePath = Config.Shared.EmulatorPath;
        if (string.IsNullOrWhiteSpace(emulatorFilePath) || !PreferredGameVersion.Equals(DEFAULT_GAME_VERSION)) {
            goto Configured;
        }
        
        string exeName = Path.GetFileName(emulatorFilePath);
        if (Path.GetFileNameWithoutExtension(exeName).Equals("ryujinx", StringComparison.InvariantCultureIgnoreCase)) {
            try {
                if (TkRyujinxHelper.GetSelectedUpdatePath(emulatorFilePath) is string updateFilePath) {
                    return builder
                        .WithSdCard(() => null)
                        .WithPackagedUpdate(() => [updateFilePath])
                        .WithNand(() => null)
                        .Build();
                }
                throw new Exception("No update is selected in Ryujinx. Right click the game in the emulator, go to 'Manage Title Updates' and select one.");
            }
            catch (Exception ex) {
                TkLog.Instance.LogError(ex, "Failed to detect a TotK update");
                goto Configured;
            }
        }

        try {
            if (NandFolderPaths.Any<string>(Directory.Exists)) {
                bool hasUpdateOnNand = false;
                KeySet? keys = TkKeyUtils.GetKeysFromFolder(KeysFolderPath!);
                if (keys == null) {
                    throw new Exception("Keys not found");
                }

                foreach (PathCollectionItem nandItem in NandFolderPaths)
                {
                    TkNandUtils.IsValid(keys, nandItem.Target, out bool hasUpdate);
                    hasUpdateOnNand = hasUpdate;
                    if (hasUpdateOnNand) break;
                }

                if (!hasUpdateOnNand) {
                    throw new Exception("No update on NAND");
                }

                return builder
                    .WithSdCard(() => null)
                    .WithPackagedUpdate(() => null)
                    .WithNand(() => NandFolderPaths)
                    .Build();
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Ensure your keys and a TotK update are installed on your emulator's " +
                                        "NAND. Alternatively, select a Preferred Game Version in the dump settings.");
        }

    Configured:
        return builder
            .WithSdCard(() => SdCardRootPath)
            .WithPackagedUpdate(() => PackagedUpdatePaths)
            .Build();
#else
        return TkExtensibleRomProviderBuilder.Create(checksums)
            .WithPreferredVersion(() => PreferredGameVersion is DEFAULT_GAME_VERSION ? null : PreferredGameVersion)
            .WithKeysFolder(() => KeysFolderPath)
            .WithSdCard(() => SdCardRootPath)
            .WithPackagedBaseGame(() => PackagedBaseGamePaths)
            .WithPackagedUpdate(() => PackagedUpdatePaths)
            .Build();
#endif
    }
}