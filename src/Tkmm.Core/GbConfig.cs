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
        Header = "Use Threaded Downloads",
        Description = "Use multi-threaded downloads for potentially faster downloads. Disable this if you experience network issues.",
        Group = "GameBanana Client")]
    public bool UseThreadedDownloads {
        get => DownloadHelper.Config.UseThreadedDownloads;
        set {
            OnPropertyChanging();
            DownloadHelper.Config.UseThreadedDownloads = value;
            OnPropertyChanged();
        }
    }

    [Config(
        Header = "Download Timeout Seconds",
        Description = "The maximum amount of seconds to wait for a response before failing.",
        Group = "GameBanana Client")]
    public int GameBananaTimeoutSeconds {
        get => DownloadHelper.Config.TimeoutSeconds;
        set {
            OnPropertyChanging();
            DownloadHelper.Config.TimeoutSeconds = value;
            OnPropertyChanged();
        }
    }

    [Config(
        Header = "GameBanana Download Max Retries",
        Description = "The maximum amount of times to retry a download before failing.",
        Group = "GameBanana Client")]
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
}