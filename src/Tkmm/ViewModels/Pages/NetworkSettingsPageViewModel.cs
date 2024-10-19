using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Tkmm.Managers;

namespace Tkmm.ViewModels.Pages
{
    public class NetworkSettingsPageViewModel : ReactiveObject
    {
        private ObservableCollection<Connman.WifiNetworkInfo> availableNetworks;
        private Connman.WifiNetworkInfo? selectedNetwork;
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

            // Initialize the states
            IsWifiEnabled = networkServices.IsWiFiEnabled();
            IsSshEnabled = networkServices.IsSSHEnabled();
            IsSmbEnabled = networkServices.IsSMBEnabled();

            // Define commands
            ConnectToNetworkCommand = ReactiveCommand.CreateFromTask(ConnectToNetworkAsync);
            ScanForNetworksCommand = ReactiveCommand.CreateFromTask(ScanForNetworksAsync);
            ForgetSsidCommand = ReactiveCommand.CreateFromTask(ForgetSsidAsync);
            DisconnectSsidCommand = ReactiveCommand.CreateFromTask(DisconnectSsidAsync);
            ToggleWifiCommand = ReactiveCommand.Create<bool>(ToggleWifi);
            ToggleSshCommand = ReactiveCommand.Create<bool>(ToggleSsh);
            ToggleSmbCommand = ReactiveCommand.Create<bool>(ToggleSmb);

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

        public string NetworkPassword
        {
            get => networkPassword;
            set => this.RaiseAndSetIfChanged(ref networkPassword, value);
        }

        public bool IsWifiEnabled
        {
            get => isWifiEnabled;
            set => this.RaiseAndSetIfChanged(ref isWifiEnabled, value);
        }

        public bool IsSshEnabled
        {
            get => isSshEnabled;
            set => this.RaiseAndSetIfChanged(ref isSshEnabled, value);
        }

        public bool IsSmbEnabled
        {
            get => isSmbEnabled;
            set => this.RaiseAndSetIfChanged(ref isSmbEnabled, value);
        }

        public ICommand ForgetSsidCommand { get; }
        public ICommand DisconnectSsidCommand { get; }
        public ICommand ConnectToNetworkCommand { get; }
        public ICommand ScanForNetworksCommand { get; }
        public ReactiveCommand<bool, Unit> ToggleWifiCommand { get; }
        public ReactiveCommand<bool, Unit> ToggleSshCommand { get; }
        public ReactiveCommand<bool, Unit> ToggleSmbCommand { get; }

        private async Task ForgetSsidAsync()
        {
            if (SelectedNetwork.HasValue && !IsDefault(SelectedNetwork.Value))
            {
                Connman.ConnmanctlForgetSsid(connman, SelectedNetwork.Value);
                await Task.Delay(3500);
                await ScanForNetworksAsync();
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
        }

        private void ToggleWifi(bool enable)
        {
            if (enable)
            {
                networkServices.EnableWiFi();
            }
            else
            {
                networkServices.DisableWiFi();
            }
            IsWifiEnabled = networkServices.IsWiFiEnabled();
        }

        private void ToggleSsh(bool enable)
        {
            if (enable)
            {
                networkServices.EnableSSH();
            }
            else
            {
                networkServices.DisableSSH();
            }
            IsSshEnabled = networkServices.IsSSHEnabled();
        }

        private void ToggleSmb(bool enable)
        {
            if (enable)
            {
                networkServices.EnableSMB();
            }
            else
            {
                networkServices.DisableSMB();
            }
            IsSmbEnabled = networkServices.IsSMBEnabled();
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