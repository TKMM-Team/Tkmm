using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Tkmm.Core.Attributes;
using Tkmm.Core.Models;
using TkSharp.Data.Embedded;
using TkSharp.Extensions.LibHac;

namespace Tkmm.Core;

public sealed partial class TkConfig : ConfigModule<TkConfig>
{
    public const string DEFAULT_GAME_VERSION = "Any";

    [JsonIgnore]
    public override string Name => "totk";

    public TkConfig()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo is { Exists: true, Length: 0 }) {
            File.Delete(LocalPath);
        }

        OnSaving += () => {
            if (GameDumpFolderPaths is [PathCollectionItem item, ..]) {
                GamePath ??= item.Target;
            }

            return true;
        };
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
        Description = "The absolute path to your dumped game update file (.xci or .nsp) or split folder.",
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

    /// <summary>
    /// The original GamePath property used by NXE and other application
    /// </summary>
    public string? GamePath { get; set; }

    public TkExtensibleRomProvider CreateRomProvider()
    {   
        using Stream checksums = TkEmbeddedDataSource.GetChecksumsBin();

        return TkExtensibleRomProviderBuilder.Create(checksums)
            .WithPreferredVersion(() => PreferredGameVersion is DEFAULT_GAME_VERSION ? null : PreferredGameVersion)
            .WithKeysFolder(() => KeysFolderPath)
            .WithExtractedGameDump(() => GamePath is null ? GameDumpFolderPaths : GameDumpFolderPaths.Append(GamePath))
            .WithSdCard(() => SdCardRootPath)
            .WithPackagedBaseGame(() => PackagedBaseGamePaths)
            .WithPackagedUpdate(() => PackagedUpdatePaths)
            .Build();
    }
}