using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Services;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana.Helpers;

namespace Tkmm.Core;

public sealed partial class GbConfig : ConfigModule<GbConfig>
{
    [Config(
        Header = "GbConfig_UseThreadedDownloads",
        Description = "GbConfig_UseThreadedDownloadsDescription",
        Group = "ConfigSection_GameBananaClient")]
    public bool UseThreadedDownloads {
        get => DownloadHelper.Config.UseThreadedDownloads;
        set {
            OnPropertyChanging();
            DownloadHelper.Config.UseThreadedDownloads = value;
            OnPropertyChanged();
        }
    }

    [Config(
        Header = "GbConfig_DownloadTimeoutSeconds",
        Description = "GbConfig_DownloadTimeoutSecondsDescription",
        Group = "ConfigSection_GameBananaClient")]
    public int GameBananaTimeoutSeconds {
        get => DownloadHelper.Config.TimeoutSeconds;
        set {
            OnPropertyChanging();
            DownloadHelper.Config.TimeoutSeconds = value;
            OnPropertyChanged();
        }
    }

    [Config(
        Header = "GbConfig_GameBananaDownloadMaxRetries",
        Description = "GbConfig_GameBananaDownloadMaxRetriesDescription",
        Group = "ConfigSection_GameBananaClient")]
    public int GameBananaMaxRetries {
        get => DownloadHelper.Config.MaxRetries;
        set {
            OnPropertyChanging();
            DownloadHelper.Config.MaxRetries = value;
            OnPropertyChanged();
        }
    }

    [Config(
        Header = "GbConfig_GameBananaPollIntervalMinutes",
        Description = "GbConfig_GameBananaPollIntervalMinutesDescription",
        Group = "ConfigSection_GameBananaClient")]
    [ObservableProperty]
    public partial int? GameBananaPollIntervalMinutes { get; set; } = 5;

    public string? PairedSecretKey { get; set; }

    public string? PairedUserId { get; set; }

    public override void Load(ref GbConfig module)
    {
        try {
            base.Load(ref module);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, string.Format(Locale["Config_ErrorFailedToLoadConfig"], nameof(GbConfig)));
            module = new GbConfig();
        }
    }

    public override string Translate(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? input : Locale[input];
    }

    partial void OnGameBananaPollIntervalMinutesChanged(int? value)
    {
        GameBananaRemoteInstallService.SetInterval(value);
    }
}