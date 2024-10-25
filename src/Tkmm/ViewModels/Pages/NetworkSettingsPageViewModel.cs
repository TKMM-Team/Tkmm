using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI;
using Tkmm.Core;
using Tkmm.Managers;

namespace Tkmm.ViewModels.Pages
{
    public class NetworkSettingsPageViewModel : ReactiveObject
    {
        public string IpAddress => _connmanInstance.IpAddress ?? "N/A";
        public string Netmask => _connmanInstance.Netmask ?? "N/A";
        public string Gateway => _connmanInstance.Gateway ?? "N/A";
        public string MacAddress => _connmanInstance.MacAddress ?? "N/A";
        private string _networkPassword = string.Empty;
        private string? _selectedNetworkId;
        private bool _isBusy;
        private bool _isConnecting;
        private bool _isScanning;
        private bool _suspendUIUpdates;
        private bool _isWifiEnabled;
        private bool _isSshEnabled;
        private bool _isSmbEnabled;
        private bool _disposed;
        private readonly Connman _connmanInstance;
        private readonly Connman.ConnmanT _connman;
        private readonly System.Timers.Timer _networkUpdateTimer;
        private CancellationTokenSource? _cancellationTokenSource;
        private List<Connman.WifiNetworkInfo> _availableNetworks;
        private Connman.WifiNetworkInfo? _selectedNetwork;
        private Connman.WifiNetworkInfo? _connectedNetwork;

        public ReactiveCommand<Unit, Unit> CancelConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectToNetworkCommand { get; }
        public ReactiveCommand<Unit, Unit> DisconnectSsidCommand { get; }
        public ReactiveCommand<Unit, Unit> ForgetSsidCommand { get; }
        public ReactiveCommand<Unit, Unit> ScanForNetworksCommand { get; }

        public NetworkSettingsPageViewModel()
        {
            _connmanInstance = new Connman();
            _connman = Connman.ConnmanctlInit();
            _availableNetworks = new List<Connman.WifiNetworkInfo>();

            _isWifiEnabled = NetworkServices.IsWifiEnabled();
            _isSshEnabled = NetworkServices.IsSshEnabled();
            _isSmbEnabled = NetworkServices.IsSmbEnabled();

            CancelConnectCommand = ReactiveCommand.Create(CancelConnect, outputScheduler: RxApp.MainThreadScheduler);
            ConnectToNetworkCommand = ReactiveCommand.CreateFromTask(async () => await ConnectToNetworkAsync());
            DisconnectSsidCommand = ReactiveCommand.CreateFromTask(async () => await DisconnectSsidAsync());
            ForgetSsidCommand = ReactiveCommand.CreateFromTask(async () => await ForgetSsidAsync());
            ScanForNetworksCommand = ReactiveCommand.CreateFromTask(async () => await ScanForNetworksAsync(true));

            _connmanInstance.NetworkDetailsChanged += RefreshNetworkDetails;

            this.WhenAnyValue(
                x => x.IpAddress,
                x => x.Netmask,
                x => x.Gateway
            ).Throttle(TimeSpan.FromMilliseconds(2500))
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                Task.Run(async () => await UpdateAvailableNetworksAsync());
            });

            ConnectToNetworkCommand.ThrownExceptions.Subscribe(ex =>
            {
                Trace.WriteLine($"Unhandled exception: {ex.Message}");
                App.Toast($"Unhandled error: {ex.Message}", "Error", NotificationType.Error, TimeSpan.FromSeconds(3));
            });

            this.WhenAnyValue(x => x.AvailableNetworks)
            .Subscribe(networks =>
            {
                foreach (var network in networks)
                {
                    network.PropertyChanged += OnNetworkPropertyChanged;
                }
            });

            if (_isWifiEnabled)
            {
                IsScanning = true;
                Task.Run(async () => await UpdateAvailableNetworksAsync());
                IsScanning = false;
            }

            _networkUpdateTimer = new System.Timers.Timer(2500) { AutoReset = false };
            _networkUpdateTimer.Elapsed += (_, _) => {
                try { Task.Run(async () => await RefreshAndUpdateNetworkStatusAsync()); } 
                catch (Exception ex) { Trace.WriteLine($"Error: {ex.Message}"); } 
                finally { _networkUpdateTimer.Start(); }
            };
            _networkUpdateTimer.Start();
        }

        private async void OnNetworkPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Connman.WifiNetworkInfo.Connected))
            {
                Task.Run(() => _connmanInstance.ConnmanctlRefreshServicesAsync(_connman));
            }
        }

        public List<Connman.WifiNetworkInfo> AvailableNetworks
        {
            get => _availableNetworks;
            private set => this.RaiseAndSetIfChanged(ref _availableNetworks, value);
        }

        public bool IsConnecting
        {
            get => _isConnecting;
            private set => this.RaiseAndSetIfChanged(ref _isConnecting, value);
        }

        public bool IsScanning
        {
            get => _isScanning;
            private set => this.RaiseAndSetIfChanged(ref _isScanning, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        public Connman.WifiNetworkInfo? SelectedNetwork
        {
            get => _selectedNetwork;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedNetwork, value);
                if (value != null)
                {
                    _selectedNetworkId = value.NetId;
                }
            }
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
                        UpdateAvailableNetworksAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        CancelConnect();
                        NetworkServices.DisableWifi();
                        AvailableNetworks = new List<Connman.WifiNetworkInfo>();
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

        private async Task ForgetSsidAsync()
        {
            if (_selectedNetwork != null && !IsDefault(_selectedNetwork))
            {
                IsBusy = true;
                Connman.ConnmanctlForgetSsid(_connman, _selectedNetwork);
                await Task.Delay(50);
                _ = Task.Run(async () => await UpdateAvailableNetworksAsync());
                Trace.WriteLine($"Settings for SSID {_selectedNetwork.Ssid} have been removed.");
                IsBusy = false;
            }
        }

        private async Task DisconnectSsidAsync()
        {
            if (_selectedNetwork != null)
            {
                IsBusy = true;
                await Task.Run(() =>
                {
                    _connmanInstance.ConnmanctlDisconnectSsid(_connman, _selectedNetwork);
                });
                _ = Task.Run(async () => await UpdateAvailableNetworksAsync());
                IsBusy = false;
            }
        }

        private async Task RefreshAndUpdateNetworkStatusAsync()
        {
            Trace.WriteLine("Refreshing and updating network status");
            await Task.Run(async () => await _connmanInstance.ConnmanctlRefreshServicesAsync(_connman));
            var networks = Connman.ConnmanctlGetSsids(_connman).NetList;
            var updatedNetworks = networks.Where(n => !string.IsNullOrEmpty(n.Ssid)).ToList();
            var networkList = new List<Connman.WifiNetworkInfo>(AvailableNetworks);
            bool hasChanges = false;

            foreach (var network in updatedNetworks)
            {
                var existingNetwork = networkList.FirstOrDefault(a => a.NetId == network.NetId);
                if (existingNetwork == null && !_suspendUIUpdates && network.Connected)
                {
                    networkList.Add(network);
                    hasChanges = true;
                }
                else if (existingNetwork != null && existingNetwork.Connected != network.Connected)
                {
                    existingNetwork.Connected = network.Connected;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                AvailableNetworks = networkList
                    .OrderByDescending(n => n.Connected)
                    .ToList();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (!string.IsNullOrEmpty(_selectedNetworkId))
                    {
                        var reselectedNetwork = AvailableNetworks.FirstOrDefault(n => n.NetId == _selectedNetworkId);
                        SelectedNetwork = reselectedNetwork;
                    }

                    this.RaisePropertyChanged(nameof(AvailableNetworks));
                    this.RaisePropertyChanged(nameof(SelectedNetwork));
                });
            }
        }

        private async Task ConnectToNetworkAsync()
        {
            if (_selectedNetwork == null) return;
            IsConnecting = true;
            _suspendUIUpdates = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            var startTime = DateTime.UtcNow;
            var network = _selectedNetwork;
            network.Passphrase = _networkPassword;
            bool initialSavedPassword = network.SavedPassword;
            bool isConnected = false;
            AppStatus.Set($"Connecting to {network.Ssid}", "fa-solid fa-wifi", isWorkingStatus: true);

            if (!initialSavedPassword)
            {
                await Task.Run(() => _connmanInstance.CreateNetworkConfigAsync(network, _connman));
                await Task.Run(() => _connmanInstance.ConnmanctlRestartAsync(_connman));
            }

            try
            {
                await Task.Run(() =>
                {
                    while (!isConnected && (DateTime.UtcNow - startTime).TotalSeconds < 60)
                    {
                        if (!initialSavedPassword)
                        {
                            Task.Run(() => _connmanInstance.ConnmanctlScanAsync(_connman)).Wait();
                        }
                        Task.Delay(1000).Wait();
                        _connmanInstance.ConnmanctlConnectSsid(_connman, network);

                        for (var i = 0; i < 50; i++)
                        {
                            Trace.WriteLine($"Connecting loop {i}");
                            cancellationToken.ThrowIfCancellationRequested();
                            Task.Run(() => RefreshAndUpdateNetworkStatusAsync()).Wait();
                            ReselectNetwork();
                            Task.Delay(300, cancellationToken).Wait();

                            _connectedNetwork = AvailableNetworks.FirstOrDefault(n => n.NetId == _selectedNetwork?.NetId && n.Connected);

                            if (_connectedNetwork == null || _connectedNetwork.Ssid != network.Ssid)
                                continue;

                            isConnected = true;
                            break;
                        }
                    }
                });

                if (isConnected)
                {
                    await Task.Run(async () => await UpdateAvailableNetworksAsync());

                    App.Toast(
                        $"Successfully connected to {network.Ssid}", "WiFi", NotificationType.Success, TimeSpan.FromSeconds(3)
                    );

                    Trace.WriteLine($"Successfully connected to {network.Ssid}");
                }
            }
            catch (OperationCanceledException)
            {
                App.Toast("Connection attempt was canceled.", "WiFi", NotificationType.Information, TimeSpan.FromSeconds(3));
                Trace.WriteLine("Connection attempt was canceled.");
            }
            finally
            {
                _suspendUIUpdates = false;
                HandleConnectionEnd(isConnected, initialSavedPassword, network);
                IsConnecting = false;
                AppStatus.Reset();
                _cancellationTokenSource = null;
            }
        }

        private void HandleConnectionEnd(bool isConnected, bool initialSavedPassword, Connman.WifiNetworkInfo network)
        {
            if (!isConnected || _cancellationTokenSource?.IsCancellationRequested == true)
            {
                if (!initialSavedPassword)
                {
                    Connman.ConnmanctlForgetSsid(_connman, network);
                }

                if (!isConnected && !_cancellationTokenSource?.IsCancellationRequested == true)
                {
                    Task.Run(async () => await UpdateAvailableNetworksAsync());
                    App.Toast(
                        $"Failed to connect to {network.Ssid}.\n\nPlease verify your password and try again.", "WiFi", NotificationType.Error, TimeSpan.FromSeconds(3)
                    );
                    Trace.WriteLine($"Failed to connect to {network.Ssid}");
                }
            }
        }

        private void CancelConnect()
        {
            _cancellationTokenSource?.Cancel();
            Task.Run(async () => await DisconnectSsidAsync());
            Task.Run(async () => await UpdateAvailableNetworksAsync());
            IsBusy = false;
            IsConnecting = false;
        }

        private async Task ScanForNetworksAsync(bool updateStatus = false)
        {
            if (!_isConnecting)
            {
                IsBusy = true;
                IsScanning = true;
            }
            if (updateStatus)
            {
                AppStatus.Set("Scanning for networks", "fa-solid fa-radar", isWorkingStatus: true);
            }
            await UpdateAvailableNetworksAsync();

            if (updateStatus)
            {
                AppStatus.Set("Scan completed", "fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
                App.Toast("Scan completed", "WiFi", NotificationType.Information, TimeSpan.FromSeconds(1));
            }
            if (!_isConnecting)
            {
                IsBusy = false;
                IsScanning = false;
            }
        }

        private async Task UpdateAvailableNetworksAsync()
        {
            if (_suspendUIUpdates)
                return;

            await _connmanInstance.ConnmanctlScanAsync(_connman);
            await _connmanInstance.ConnmanctlRefreshServicesAsync(_connman);

            var networks = await Task.Run(() => Connman.ConnmanctlGetSsids(_connman).NetList);

            var allNetworks = networks
                .Where(n => !string.IsNullOrEmpty(n.Ssid))
                .OrderByDescending(n => n.Connected)
                .ToList();

            if (!AvailableNetworks.SequenceEqual(allNetworks))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AvailableNetworks = allNetworks;

                    if (!string.IsNullOrEmpty(_selectedNetworkId))
                    {
                        var reselectedNetwork = allNetworks.FirstOrDefault(n => n.NetId == _selectedNetworkId);
                        SelectedNetwork = reselectedNetwork;
                    }

                    this.RaisePropertyChanged(nameof(AvailableNetworks));
                    this.RaisePropertyChanged(nameof(SelectedNetwork));
                });
            }
        }
        

        private void ReselectNetwork()
        {
            if (!string.IsNullOrEmpty(_selectedNetworkId))
            {
                var reselectedNetwork = AvailableNetworks.FirstOrDefault(n => n.NetId == _selectedNetworkId);
                
                if (reselectedNetwork != null && SelectedNetwork != reselectedNetwork)
                {
                    Dispatcher.UIThread.InvokeAsync(() => SelectedNetwork = reselectedNetwork);
                }
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