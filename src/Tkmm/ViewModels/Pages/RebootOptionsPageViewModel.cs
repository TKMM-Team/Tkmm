using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Tkmm.Views.Pages;
using TkSharp.Core;
#if SWITCH
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using Tkmm.Models.MenuModels;
#endif

namespace Tkmm.ViewModels.Pages;

public partial class RebootOptionsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RebootOption> _launchOptions = new();

    [ObservableProperty]
    private ObservableCollection<RebootOption> _configOptions = new();

    [ObservableProperty]
    private ObservableCollection<RebootOption> _umsOptions = new();

#if DEBUG
    private const string BOOT_DISK_PATH = "V:\\";
#else
    private const string BOOT_DISK_PATH = "/flash/";
#endif
    
    private readonly Bitmap? _fallbackIcon = ConvertBmpToBitmap("bootloader/res/icon_switch.bmp");

    public RebootOptionsPageViewModel()
    {
        LoadRebootOptions();
    }

    private void LoadRebootOptions()
    {
        LoadLaunchOptions();
        LoadConfigOptions();
        LoadUmsOptions();
    }

    private void LoadLaunchOptions()
    {
        LaunchOptions.Clear();
        var hekateIplPath = Path.Combine(BOOT_DISK_PATH, "bootloader", "hekate_ipl.ini");
        
        if (File.Exists(hekateIplPath)) {
            var sections = ParseIniSections(hekateIplPath);
            int index = 1;
            foreach (var section in sections) {
                var icon = ConvertBmpToBitmap(section.IconPath) ?? _fallbackIcon;
                LaunchOptions.Add(new RebootOption {
                    Name = section.Name,
                    Type = RebootType.Launch,
                    Index = index,
                    Icon = icon
                });
                index++;
            }
        }
    }

    private void LoadConfigOptions()
    {
        ConfigOptions.Clear();
        var iniDirectory = Path.Combine(BOOT_DISK_PATH, "bootloader", "ini");
        
        if (Directory.Exists(iniDirectory)) {
            var iniFiles = Directory.GetFiles(iniDirectory, "*.ini")
                                   .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
                                   .ToArray();
            
            var allSections = new List<(string Name, string? IconPath, string FileName)>();
            
            foreach (var iniFile in iniFiles) {
                var sections = ParseIniSections(iniFile);
                foreach (var section in sections) {
                    allSections.Add((section.Name, section.IconPath, Path.GetFileName(iniFile)));
                }
            }
            
            int globalIndex = 1;
            foreach (var section in allSections) {
                var icon = ConvertBmpToBitmap(section.IconPath) ?? _fallbackIcon;
                ConfigOptions.Add(new RebootOption {
                    Name = section.Name,
                    Type = RebootType.Config,
                    Index = globalIndex,
                    Icon = icon
                });
                globalIndex++;
            }
        }
    }

    private void LoadUmsOptions()
    {
        UmsOptions.Clear();
        var umsOptions = new[] {
            "SD Card",
            "eMMC BOOT0",
            "eMMC BOOT1", 
            "eMMC GPP",
            "emuMMC BOOT0",
            "emuMMC BOOT1",
            "emuMMC GPP"
        };

        for (int i = 0; i < umsOptions.Length; i++) {
            UmsOptions.Add(new RebootOption {
                Name = umsOptions[i],
                Type = RebootType.Ums,
                Index = i
            });
        }
    }

    private List<(string Name, string? IconPath)> ParseIniSections(string iniFilePath)
    {
        var sections = new List<(string Name, string? IconPath)>();
        
        try {
            var lines = File.ReadAllLines(iniFilePath);
            string? currentSection = null;
            string? currentIcon = null;
            
            foreach (var line in lines) {
                var trimmedLine = line.Trim();
                
                if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']')) {
                    if (currentSection != null && currentSection != "config") {
                        sections.Add((currentSection, currentIcon));
                    }
                    
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    currentIcon = null;
                }
                else if (currentSection != null && currentSection != "config" && trimmedLine.StartsWith("icon=", StringComparison.OrdinalIgnoreCase)) {
                    currentIcon = trimmedLine.Substring(5).Trim();
                }
            }
            
            if (currentSection != null && currentSection != "config") {
                sections.Add((currentSection, currentIcon));
            }
        }
        catch (Exception _) {
            TkLog.Instance.LogError(_, "Failed to parse Hekate INI files");
        }
        
        return sections;
    }

    private static Bitmap? ConvertBmpToBitmap(string? bmpPath)
    {
        if (string.IsNullOrEmpty(bmpPath)) {
            return null;
        }
        
        var fullBmpPath = Path.Combine(BOOT_DISK_PATH, bmpPath);
        
        if (!File.Exists(fullBmpPath)) {
            return null;
        }

        try {
#if SWITCH
            using var image = Image.Load(fullBmpPath);
            using var stream = new MemoryStream();
            image.Save(stream, new PngEncoder());
            stream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(stream);
#else
            return null;
#endif
        }
        catch (Exception ex) {
            TkLog.Instance.LogError($"Failed to convert BMP {bmpPath}: {ex.Message}");
            return null;
        }
    }
    
    [RelayCommand]
    private void SelectLaunchOption(RebootOption option)
    {
        ExecuteR2CCommand("launch", option.Index.ToString());
    }

    [RelayCommand]
    private void SelectConfigOption(RebootOption option)
    {
        ExecuteR2CCommand("config", option.Index.ToString());
    }

    [RelayCommand]
    private void SelectUmsOption(RebootOption option)
    {
        ExecuteR2CCommand("ums", option.Index.ToString());
    }

    [RelayCommand]
    private void RebootToBootloader()
    {
        ExecuteR2CCommand("bootloader");
    }

    [RelayCommand]
    private void NormalReboot()
    {
        ExecuteR2CCommand("normal");
    }

    [RelayCommand]
    private void Shutdown()
    {
#if SWITCH
        NxMenuModel.Shutdown();
#endif
    }

    private void ExecuteR2CCommand(string action, string? param = null)
    {
        try {
            var startInfo = new ProcessStartInfo
            {
                FileName = "r2c",
                Arguments = param != null ? $"{action} {param}" : action,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
        catch (Exception _)
        {
            TkLog.Instance.LogError(_, "Failed to execute R2C command");
        }
    }

    private static ContentDialog? _currentDialog;

    public static async void ShowReboot2ConfigPopup()
    {
        try {
            if (_currentDialog != null) {
                _currentDialog.Hide(ContentDialogResult.None);
                _currentDialog = null;
                return;
            }

            var rebootOptionsView = new RebootOptionsPageView();

            var dialog = new ContentDialog {
                Title = Locale[TkLocale.Menu_Nx],
                Content = rebootOptionsView,
                CloseButtonText = Locale[TkLocale.Action_Cancel],
                CornerRadius = new CornerRadius(12),
                DefaultButton = ContentDialogButton.Close
            };

            _currentDialog = dialog;

            await dialog.ShowAsync();

            _currentDialog = null;
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Error occurred while showing the reboot options dialog.");
            _currentDialog = null;
        }
    }
}

public class RebootOption
{
    public string Name { get; set; } = string.Empty;
    public RebootType Type { get; set; }
    public int Index { get; set; }
    public Bitmap? Icon { get; set; }
}

public enum RebootType
{
    Launch,
    Config,
    Ums
}