using Avalonia.Controls.Notifications;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using Tkmm.Core;
using Tkmm.Managers;

namespace Tkmm.ViewModels.Pages
{
    public class NetworkSettingsPageViewModel : ReactiveObject, IDisposable
    {
        public string IpAddress => _connmanInstance.IpAddress ?? "N/A";
        public string Netmask => _connmanInstance.Netmask ?? "N/A";
        public string Gateway => _connmanInstance.Gateway ?? "N/A";
        public string MacAddress => _connmanInstance.MacAddress ?? "N/A";
        private Connman.WifiNetworkInfo? _selectedNetwork;
        private Connman.WifiNetworkInfo? _connectedNetwork;
        private string _networkPassword = string.Empty;
        private bool _isWifiEnabled;
        private bool _isSshEnabled;
        private bool _isSmbEnabled;
        private bool _disposed;
        private readonly Connman _connmanInstance;
        private readonly Connman.ConnmanT _connman;
        private readonly System.Timers.Timer _networkUpdateTimer;
        public ObservableCollection<Connman.WifiNetworkInfo> AvailableNetworks { get; init; }

        private bool _isConnecting;
        public bool IsConnecting
        {
            get => _isConnecting;
            private set => this.RaiseAndSetIfChanged(ref _isConnecting, value);
        }

        public NetworkSettingsPageViewModel()
        {
            _connmanInstance = new Connman();
            _connman = Connman.ConnmanctlInit();
            AvailableNetworks = new();

            _isWifiEnabled = NetworkServices.IsWifiEnabled();
            _isSshEnabled = NetworkServices.IsSshEnabled();
            _isSmbEnabled = NetworkServices.IsSmbEnabled();

            ConnectToNetworkCommand = ReactiveCommand.CreateFromTask(ConnectToNetworkAsync);
            ScanForNetworksCommand = ReactiveCommand.CreateFromTask(() => ScanForNetworksAsync(true));
            ForgetSsidCommand = ReactiveCommand.CreateFromTask(ForgetSsidAsync);
            DisconnectSsidCommand = ReactiveCommand.CreateFromTask(DisconnectSsidAsync);

            if (_isWifiEnabled)
            {
                _ = ScanForNetworksAsync(true);
            }

            _connmanInstance.NetworkDetailsChanged += RefreshNetworkDetails;

            _networkUpdateTimer = new System.Timers.Timer(7500)
            {
                AutoReset = true
            };
            _networkUpdateTimer.Elapsed += (_, _) => _connmanInstance.ConnmanctlRefreshServices(_connman);
            _networkUpdateTimer.Start();

            this.WhenAnyValue(
                x => x.IpAddress,
                x => x.Netmask,
                x => x.Gateway
            ).Throttle(TimeSpan.FromMilliseconds(2500))
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                UpdateAvailableNetworks();
            });
        }

        public Connman.WifiNetworkInfo? SelectedNetwork
        {
            get => _selectedNetwork;
            set => this.RaiseAndSetIfChanged(ref _selectedNetwork, value);
        }

        public Connman.WifiNetworkInfo? ConnectedNetwork
        {
            get => _connectedNetwork;
            set => this.RaiseAndSetIfChanged(ref _connectedNetwork, value);
        }

        public string NetworkPassword
        {
            get => _networkPassword;
            set => this.RaiseAndSetIfChanged(ref _networkPassword, value);
        }

        public bool IsWifiEnabled
        {
            get => _isWifiEnabled;
            set
            {
                if (_isWifiEnabled != value)
                {
                    this.RaiseAndSetIfChanged(ref _isWifiEnabled, value);
                    if (value)
                    {
                        NetworkServices.EnableWifi();
                        _ = ScanForNetworksAsync();
                    }
                    else
                    {
                        NetworkServices.DisableWifi();
                        AvailableNetworks.Clear();
                        UpdateAvailableNetworks();
                    }
                }
            }
        }

        public bool IsSshEnabled
        {
            get => _isSshEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSshEnabled, value);
                if (value)
                {
                    NetworkServices.EnableSsh();
                }
                else
                {
                    NetworkServices.DisableSsh();
                }
            }
        }

        public bool IsSmbEnabled
        {
            get => _isSmbEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSmbEnabled, value);
                if (value)
                {
                    NetworkServices.EnableSmb();
                }
                else
                {
                    NetworkServices.DisableSmb();
                }
            }
        }

        public ICommand ForgetSsidCommand { get; }
        public ICommand DisconnectSsidCommand { get; }
        public ICommand ConnectToNetworkCommand { get; }
        public ICommand ScanForNetworksCommand { get; }

        private async Task ForgetSsidAsync()
        {
            if (_selectedNetwork != null && !IsDefault(_selectedNetwork.Value))
            {
                await Task.Run(() => 
                {
                    Connman.ConnmanctlForgetSsid(_connman, _selectedNetwork.Value);
                    Trace.WriteLine($"Settings for SSID {_selectedNetwork.Value.Ssid} have been removed.");
                    UpdateAvailableNetworks();
                });
            }
        }

        private async Task DisconnectSsidAsync()
        {
            if (_selectedNetwork != null)
            {
                await Task.Run(() => 
                {
                    _connmanInstance.ConnmanctlDisconnectSsid(_connman, _selectedNetwork.Value);
                });
            }
        }

        private async Task ConnectToNetworkAsync()
        {
            if (_selectedNetwork == null || IsDefault(_selectedNetwork.Value)) 
                return;

            IsConnecting = true;
            _networkUpdateTimer.Stop();

            var network = _selectedNetwork.Value;
            network.Passphrase = _networkPassword;

            AppStatus.Set($"Connecting to {network.Ssid}", "fa-solid fa-wifi", isWorkingStatus: true);

            bool isConnected = false;
            var startTime = DateTime.UtcNow;

            while (!isConnected && (DateTime.UtcNow - startTime).TotalSeconds < 60)
            {
                await _connmanInstance.ConnmanctlConnectSsidAsync(_connman, network);

                for (var i = 0; i < 50; i++)
                {
                    await Task.Delay(200);
                    Connman.ConnmanctlGetConnectedSsid(_connman);

                    if (_connectedNetwork == null || _connectedNetwork.Value.Ssid != network.Ssid)
                        continue;

                    App.Toast(
                        $"Successfully connected to {network.Ssid}", "WiFi", NotificationType.Success, TimeSpan.FromSeconds(3)
                    );
                    Trace.WriteLine($"Successfully connected to {network.Ssid}");
                    isConnected = true;
                    break;
                }

                if (!isConnected)
                {
                    await ScanForNetworksAsync();
                }
            }

            if (!isConnected)
            {
                Connman.ConnmanctlForgetSsid(_connman, network);
                UpdateAvailableNetworks();
                App.Toast(
                    $"Failed to connect to {network.Ssid}.\n\nPlease verify your password and try again.", "WiFi", NotificationType.Error, TimeSpan.FromSeconds(3)
                );
                Trace.WriteLine($"Failed to connect to {network.Ssid}");
            }

            AppStatus.Reset();
            _networkUpdateTimer.Start();
            IsConnecting = false;
        }

        private async Task ScanForNetworksAsync(bool updateStatus = false)
        {
            if (updateStatus)
            {
                AppStatus.Set("Scanning for networks",
                    "fa-solid fa-radar",
                    isWorkingStatus: true
                );
            }

            _connmanInstance.ConnmanctlScan(_connman);
            await Task.Delay(3500);
            _connmanInstance.ConnmanctlRefreshServices(_connman);
            _connectedNetwork = AvailableNetworks.FirstOrDefault(n => n.Connected);

            if (updateStatus)
            {
                UpdateAvailableNetworks();
                AppStatus.Set("Scan completed",
                    "fa-circle-check",
                    isWorkingStatus: false,
                    temporaryStatusTime: 1.5
                );
            }
        }

        private void UpdateAvailableNetworks()
        {
            _connmanInstance.ConnmanctlRefreshServices(_connman);
            var networks = Connman.ConnmanctlGetSsids(_connman).NetList;
            AvailableNetworks.Clear();
            foreach (var network in networks.Where(n => !string.IsNullOrEmpty(n.Ssid)))
            {
                AvailableNetworks.Add(network);
            }
        }

        private static bool IsDefault(Connman.WifiNetworkInfo netinfo)
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _networkUpdateTimer.Stop();
                    _networkUpdateTimer.Dispose();
                }
                _disposed = true;
            }
        }
    }
}