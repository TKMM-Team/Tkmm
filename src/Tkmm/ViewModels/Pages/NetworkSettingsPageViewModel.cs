using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using Tkmm.Managers;

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

        public NetworkSettingsPageViewModel()
        {
            connman = Connman.ConnmanctlInit();
            networkServices = new NetworkServices();
            AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>();

            IsWifiEnabled = networkServices.IsWiFiEnabled();
            IsSshEnabled = networkServices.IsSSHEnabled();
            IsSmbEnabled = networkServices.IsSMBEnabled();

            ConnectToNetworkCommand = ReactiveCommand.CreateFromTask(ConnectToNetworkAsync);
            ScanForNetworksCommand = ReactiveCommand.CreateFromTask(ScanForNetworksAsync);
            ForgetSsidCommand = ReactiveCommand.CreateFromTask(ForgetSsidAsync);
            DisconnectSsidCommand = ReactiveCommand.CreateFromTask(DisconnectSsidAsync);

            ScanForNetworksCommand.Execute(null);
        }

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
            set
            {
                this.RaiseAndSetIfChanged(ref connectedNetwork, value);
                UpdateNetworkDetails();
            }
        }

        public string IpAddress { get; private set; }
        public string Netmask { get; private set; }
        public string Gateway { get; private set; }
        public string MacAddress { get; private set; }

        private void UpdateNetworkDetails()
        {
            IpAddress = connectedNetwork?.IpAddress ?? "N/A";
            Netmask = connectedNetwork?.Netmask ?? "N/A";
            Gateway = connectedNetwork?.Gateway ?? "N/A";
            MacAddress = connectedNetwork?.MacAddress ?? "N/A";

            this.RaisePropertyChanged(nameof(IpAddress));
            this.RaisePropertyChanged(nameof(Netmask));
            this.RaisePropertyChanged(nameof(Gateway));
            this.RaisePropertyChanged(nameof(MacAddress));
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
                this.RaiseAndSetIfChanged(ref isWifiEnabled, value);
                if (value)
                {
                    networkServices.EnableWiFi();
                }
                else
                {
                    networkServices.DisableWiFi();
                    AvailableNetworks.Clear();
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
                UpdateAvailableNetworks();
            }
        }

        private async Task DisconnectSsidAsync()
        {
            if (SelectedNetwork.HasValue)
            {
                Connman.ConnmanctlDisconnectSsid(connman, SelectedNetwork.Value);
                await ScanForNetworksAsync();
            }
        }

        private async Task ConnectToNetworkAsync()
        {
            if (SelectedNetwork.HasValue && !IsDefault(SelectedNetwork.Value))
            {
                var network = SelectedNetwork.Value;
                network.Passphrase = NetworkPassword;
                Connman.ConnmanctlConnectSsid(connman, network);
                await Task.Delay(3500);
                await ScanForNetworksAsync();
            }
        }

        private async Task ScanForNetworksAsync()
        {
            Connman.ConnmanctlScan(connman);
            await Task.Delay(3500);
            UpdateAvailableNetworks();
            ConnectedNetwork = AvailableNetworks.FirstOrDefault(n => n.Connected);
        }

        private void UpdateAvailableNetworks()
        {
            var networks = Connman.ConnmanctlGetSsids(connman)?.NetList;
            AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>(
                networks?.Where(n => !string.IsNullOrEmpty(n.Ssid)) ?? Enumerable.Empty<Connman.WifiNetworkInfo>());
        }

        private bool IsDefault(Connman.WifiNetworkInfo netinfo)
        {
            return string.IsNullOrEmpty(netinfo.Ssid) && string.IsNullOrEmpty(netinfo.NetId);
        }
    }
}