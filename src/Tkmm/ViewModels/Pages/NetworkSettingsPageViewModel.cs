
using Avalonia.Controls.Notifications;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Timers;
using System.Windows.Input;
using ReactiveUI;
using Tkmm.Core;
using Tkmm.Managers;
using Tkmm;

namespace Tkmm.ViewModels.Pages
{
    public class NetworkSettingsPageViewModel : ReactiveObject
    {
        private ObservableCollection<Connman.WifiNetworkInfo> availableNetworks;
        private Connman.WifiNetworkInfo? selectedNetwork;
        private Connman.WifiNetworkInfo? connectedNetwork;
        private string networkPassword;
        private bool isWifiEnabled;
        private bool isSshEnabled;
        private bool isSmbEnabled;
        private readonly Connman.ConnmanT connman;
        private readonly NetworkServices networkServices;
        private readonly System.Timers.Timer networkUpdateTimer;

        public NetworkSettingsPageViewModel()
        {
            connman = Connman.ConnmanctlInit();
            networkServices = new NetworkServices();
            AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>();

            isWifiEnabled = networkServices.IsWiFiEnabled();

            isSshEnabled = networkServices.IsSSHEnabled();
            isSmbEnabled = networkServices.IsSMBEnabled();

            ConnectToNetworkCommand = ReactiveCommand.CreateFromTask(ConnectToNetworkAsync);
            ScanForNetworksCommand = ReactiveCommand.CreateFromTask(() => ScanForNetworksAsync(true));
            ForgetSsidCommand = ReactiveCommand.CreateFromTask(ForgetSsidAsync);
            DisconnectSsidCommand = ReactiveCommand.CreateFromTask(DisconnectSsidAsync);

            if (isWifiEnabled)
            {
                ScanForNetworksAsync(true);
            }

            Connman.NetworkDetailsChanged += RefreshNetworkDetails;

            networkUpdateTimer = new System.Timers.Timer(7500);
            networkUpdateTimer.Elapsed += (sender, e) => Connman.ConnmanctlRefreshServices(connman);
            networkUpdateTimer.AutoReset = true;
            networkUpdateTimer.Start();

            this.WhenAnyValue(
                x => x.IpAddress,
                x => x.Netmask,
                x => x.Gateway
            ).Throttle(TimeSpan.FromMilliseconds(2500))
             .DistinctUntilChanged()
             .Subscribe(_ => UpdateAvailableNetworks());
        }

        public string IpAddress => Connman.IpAddress;
        public string Netmask => Connman.Netmask;
        public string Gateway => Connman.Gateway;
        public string MacAddress => Connman.MacAddress;

        public ObservableCollection<Connman.WifiNetworkInfo> AvailableNetworks
        {
            get => availableNetworks;
            set => this.RaiseAndSetIfChanged(ref availableNetworks, value);
        }

        public Connman.WifiNetworkInfo? SelectedNetwork
        {
            get => selectedNetwork;
            set => this.RaiseAndSetIfChanged(ref selectedNetwork, value);
        }

        public Connman.WifiNetworkInfo? ConnectedNetwork
        {
            get => connectedNetwork;
            set => this.RaiseAndSetIfChanged(ref connectedNetwork, value);
        }

        public string NetworkPassword
        {
            get => networkPassword;
            set => this.RaiseAndSetIfChanged(ref networkPassword, value);
        }

        public bool IsWifiEnabled
        {
            get => isWifiEnabled;
            set
            {
                if (isWifiEnabled != value)
                {
                    this.RaiseAndSetIfChanged(ref isWifiEnabled, value);
                    if (value)
                    {
                        networkServices.EnableWiFi();
                        ScanForNetworksAsync();
                    }
                    else
                    {
                        networkServices.DisableWiFi();
                        AvailableNetworks.Clear();
                        UpdateAvailableNetworks();
                    }
                }
            }
        }

        public bool IsSshEnabled
        {
            get => isSshEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref isSshEnabled, value);
                if (value)
                {
                    networkServices.EnableSSH();
                }
                else
                {
                    networkServices.DisableSSH();
                }
            }
        }

        public bool IsSmbEnabled
        {
            get => isSmbEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref isSmbEnabled, value);
                if (value)
                {
                    networkServices.EnableSMB();
                }
                else
                {
                    networkServices.DisableSMB();
                }
            }
        }

        public ICommand ForgetSsidCommand { get; }
        public ICommand DisconnectSsidCommand { get; }
        public ICommand ConnectToNetworkCommand { get; }
        public ICommand ScanForNetworksCommand { get; }

        private async Task ForgetSsidAsync()
        {
            if (SelectedNetwork.HasValue && !IsDefault(SelectedNetwork.Value))
            {
                Connman.ConnmanctlForgetSsid(connman, SelectedNetwork.Value);
                Trace.WriteLine($"Settings for SSID {SelectedNetwork.Value.Ssid} have been removed.");
                UpdateAvailableNetworks();
            }
        }

        private async Task DisconnectSsidAsync()
        {
            if (SelectedNetwork.HasValue)
            {
                Connman.ConnmanctlDisconnectSsid(connman, SelectedNetwork.Value);
                UpdateAvailableNetworks();
            }
        }

        private async Task ConnectToNetworkAsync()
        {
            if (SelectedNetwork.HasValue && !IsDefault(SelectedNetwork.Value))
            {
                var network = SelectedNetwork.Value;
                network.Passphrase = NetworkPassword;

                AppStatus.Set($"Connecting to {network.Ssid}", "fa-solid fa-wifi", isWorkingStatus: true);

                bool isConnected = false;
                var startTime = DateTime.UtcNow;

                while (!isConnected && (DateTime.UtcNow - startTime).TotalSeconds < 60)
                {
                    Connman.ConnmanctlConnectSsid(connman, network);
                    await Task.Delay(5000);
                    await ScanForNetworksAsync();

                    if (ConnectedNetwork.HasValue && ConnectedNetwork.Value.Ssid == network.Ssid)
                    {
                        App.Toast(
                            $"Successfully connected to {network.Ssid}", "WiFi", NotificationType.Success, TimeSpan.FromSeconds(3)
                        );
                        Trace.WriteLine($"Successfully connected to {network.Ssid}");
                        isConnected = true;
                    }
                }

                if (!isConnected)
                {
                    Connman.ConnmanctlForgetSsid(connman, network);
                    UpdateAvailableNetworks();
                    App.Toast(
                        $"Failed to connect to {network.Ssid}.\n\nPlease verify your password and try again.", "WiFi", NotificationType.Error, TimeSpan.FromSeconds(3)
                    );
                    Trace.WriteLine($"Failed to connect to {network.Ssid}");
                }

                AppStatus.Set("Ready", "fa-regular fa-message", isWorkingStatus: false);
            }
        }

        private async Task ScanForNetworksAsync(bool updateStatus = false)
        {
            if (updateStatus)
            {
                AppStatus.Set($"Scanning for networks",
                    "fa-solid fa-radar",
                    isWorkingStatus: true
                );
            }

            Connman.ConnmanctlScan(connman);
            await Task.Delay(5000);
            UpdateAvailableNetworks();
            ConnectedNetwork = AvailableNetworks.FirstOrDefault(n => n.Connected);

            if (updateStatus)
            {
                AppStatus.Set($"Scan completed",
                    "fa-circle-check",
                    isWorkingStatus: false,
                    temporaryStatusTime: 1.5
                );
            }
        }

        private void UpdateAvailableNetworks()
        {
            Connman.ConnmanctlRefreshServices(connman);
            var networks = Connman.ConnmanctlGetSsids(connman)?.NetList;
            AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>(
                networks?.Where(n => !string.IsNullOrEmpty(n.Ssid)) ?? Enumerable.Empty<Connman.WifiNetworkInfo>());
        }

        private bool IsDefault(Connman.WifiNetworkInfo netinfo)
        {
            return string.IsNullOrEmpty(netinfo.Ssid) && string.IsNullOrEmpty(netinfo.NetId);
        }

        private void RefreshNetworkDetails()
        {
            this.RaisePropertyChanged(nameof(IpAddress));
            this.RaisePropertyChanged(nameof(Netmask));
            this.RaisePropertyChanged(nameof(Gateway));
            this.RaisePropertyChanged(nameof(MacAddress));
            this.RaisePropertyChanged(nameof(AvailableNetworks));
        }

        public void Dispose()
        {
            networkUpdateTimer?.Stop();
            networkUpdateTimer?.Dispose();
        }
    }
}