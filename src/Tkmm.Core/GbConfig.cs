using System.Text.Json.Serialization;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Microsoft.Extensions.Logging;
using TkSharp.Core;
using TkSharp.Extensions.GameBanana.Helpers;

namespace Tkmm.Core;

public sealed class GbConfig : ConfigModule<GbConfig>
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

    public override void Load(ref GbConfig module)
    {
        try {
            base.Load(ref module);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Failed to load config: '{ConfigName}'", nameof(GbConfig));
            module = new GbConfig();
        }
    }

    public override string Translate(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? input : Locale[input];
    }
}