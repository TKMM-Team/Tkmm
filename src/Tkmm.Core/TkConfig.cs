using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

namespace Tkmm.Core;

public sealed partial class TkConfig : ConfigModule<TkConfig>
{
    public override string Name => "totk";
    
    public TkConfig()
    {
        FileInfo configFileInfo = new(LocalPath);
        if (configFileInfo is { Exists: true, Length: 0 }) {
            File.Delete(LocalPath);
        }
    }
    
    [ObservableProperty]
    private string _gameLanguage = "USen";
    
    [ObservableProperty]
    [property: Config(
        Header = "Keys Folder Path",
        Description = "The absolute path to the folder containing your dumped Switch keys.",
        Group = "Game Dump")]
    private string? _keysFolderPath;

    [ObservableProperty]
    [property: Config(
        Header = "Base Game File Path",
        Description = "The absolute path to your dumped base game file (XCI/NSP).",
        Group = "Game Dump")]
    private string? _baseGameFilePath;

    // TODO: Support version dropdown
    [ObservableProperty]
    [property: Config(
        Header = "Game Update File Path",
        Description = "The absolute path to your dumped game update file (NSP).",
        Group = "Game Dump")]
    private string? _gameUpdateFilePath;

    // TODO: Support multiple game paths
    [ObservableProperty]
    [property: Config(
        Header = "Game Dump Folder Path",
        Description = "The absolute path to your dumped ROMFS folder.",
        Group = "Game Dump")]
    [JsonPropertyName("GamePath")]
    private string? _gameDumpFolderPath;

    [ObservableProperty]
    private string? _sdCardContentsPath;
}