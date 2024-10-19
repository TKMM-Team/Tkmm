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
        private readonly Connman.ConnmanT connman;

        public NetworkSettingsPageViewModel()
        {
            connman = Connman.ConnmanctlInit();
            AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>();

            ConnectToNetworkCommand = ReactiveCommand.CreateFromTask(ConnectToNetworkAsync);
            ScanForNetworksCommand = ReactiveCommand.CreateFromTask(ScanForNetworksAsync);
            ForgetSsidCommand = ReactiveCommand.CreateFromTask(ForgetSsidAsync);
            DisconnectSsidCommand = ReactiveCommand.CreateFromTask(DisconnectSsidAsync);
            EnableWifiCommand = ReactiveCommand.CreateFromTask(() => ToggleWifiAsync(true));
            DisableWifiCommand = ReactiveCommand.CreateFromTask(() => ToggleWifiAsync(false));

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

        public ICommand ForgetSsidCommand { get; }
        public ICommand DisconnectSsidCommand { get; }
        public ICommand ConnectToNetworkCommand { get; }
        public ICommand ScanForNetworksCommand { get; }
        public ICommand EnableWifiCommand { get; }
        public ICommand DisableWifiCommand { get; }

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

        private async Task ToggleWifiAsync(bool enable)
        {
            Connman.ConnmanctlEnable(connman, enable);
            await Task.Delay(3500);
            UpdateAvailableNetworks();
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