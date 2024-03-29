﻿using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

namespace Tkmm.Core;

public partial class Config : ConfigModule<Config>
{
    static Config()
    {
        Directory.CreateDirectory(DocumentsFolder);
    }

    public static string DocumentsFolder { get; }
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TotK Mod Manager");

    private static readonly string _defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tkmm");
    private static readonly string _defaultMergedPath = Path.Combine(DocumentsFolder, "TotK Mod Manager", "Merged Output");

    public override string Name { get; } = "tkmm";

    public string StaticStorageFolder { get; }
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tkmm");

    public static Action<string>? SetTheme { get; set; }

    [ObservableProperty]
    [property: Config(
        Header = "Theme",
        Description = "",
        Group = "Application")]
    [property: DropdownConfig("Dark", "Light")]
    private string _theme = "Dark";

    [ObservableProperty]
    [property: Config(
        Header = "Show Console",
        Description = "Show the console window for additional information (restart required)",
        Group = "Application")]
    private bool _showConsole = false;

    [ObservableProperty]
    [property: Config(
        Header = "System Folder",
        Description = "The folder used to store TKMM system files.",
        Group = "Application")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "config-storage-folder",
        Title = "Storage Folder")]
    private string _storageFolder = _defaultPath;

    [ObservableProperty]
    [property: Config(
        Header = "Default Author",
        Description = "The default author used when packaging TKCL mods.",
        Group = "Packaging")]
    private string _defaultAuthor = Path.GetFileName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    [ObservableProperty]
    [property: Config(
        Header = "Merged Mod Output Folder",
        Description = "The output folder to write the final merged mod to.",
        Group = "Merging")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        InstanceBrowserKey = "config-mrged-output-folder",
        Title = "Merged Mod Output Folder")]
    private string _mergeOutput = _defaultMergedPath;

    [ObservableProperty]
    [property: Config(
        Header = "Target Language",
        Description = "The target language that MalsMerger should create an archive for.",
        Group = "Merging")]
    [property: DropdownConfig("USen", "EUen", "JPja", "EUfr", "USfr", "USes", "EUes", "EUde", "EUnl", "EUit", "KRko", "CNzh", "TWzh")]
    private string _gameLanguage = "USen";

    [ObservableProperty]
    [property: Config(
        Header = "Use Ryujinx",
        Description = "Automatically export to your Ryujinx mod folder.",
        Group = "Merging")]
    private bool _useRyujinx = false;

    [ObservableProperty]
    [property: Config(
        Header = "Use Japanese Citrus Fruit",
        Description = "Automatically export to your Japanese Citrus Fruit mod folder.",
        Group = "Merging")]
    private bool _useJapaneseCitrusFruit = false;

    partial void OnThemeChanged(string value)
    {
        SetTheme?.Invoke(value);
    }

    private static readonly string _ryujinxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx", "sdcard", "atmosphere", "contents", "0100f2c0115b6000");
    partial void OnUseRyujinxChanged(bool value)
    {
        if (Directory.Exists(_ryujinxPath)) {
            Directory.Delete(_ryujinxPath, true);
        }

        if (value == true) {
            if (Path.GetDirectoryName(_ryujinxPath) is string folder) {
                Directory.CreateDirectory(folder);
            }

            Directory.CreateSymbolicLink(_ryujinxPath, MergeOutput);
        }
    }

    private static readonly string _japaneseCitrusFruitPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "yuzu", "load", "0100F2C0115B6000", "TKMM");
    partial void OnUseJapaneseCitrusFruitChanged(bool value)
    {
        if (Directory.Exists(_japaneseCitrusFruitPath)) {
            Directory.Delete(_japaneseCitrusFruitPath, true);
        }

        if (value == true) {
            if (Path.GetDirectoryName(_japaneseCitrusFruitPath) is string folder) {
                Directory.CreateDirectory(folder);
            }

            Directory.CreateSymbolicLink(_japaneseCitrusFruitPath, MergeOutput);
        }
    }
}
