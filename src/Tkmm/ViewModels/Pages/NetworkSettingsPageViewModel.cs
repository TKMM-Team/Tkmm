using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using Tkmm.Managers;
using System.Diagnostics;

namespace Tkmm.ViewModels.Pages
{
    public class NetworkSettingsPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Connman.WifiNetworkInfo> availableNetworks;
        private Connman.WifiNetworkInfo? selectedNetwork;
        private string networkPassword;
        private readonly Connman.ConnmanT connman;

        public NetworkSettingsPageViewModel()
        {
            connman = Connman.ConnmanctlInit();
            AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>();
            ConnectToNetworkCommand = ReactiveCommand.Create(ConnectToNetwork);
            ScanForNetworksCommand = ReactiveCommand.Create(ScanForNetworks);
            ForgetSsidCommand = ReactiveCommand.Create(ForgetSsid);
            DisconnectSsidCommand = ReactiveCommand.Create(DisconnectSsid);
            ScanForNetworks();
        }

        public ObservableCollection<Connman.WifiNetworkInfo> AvailableNetworks
        {
            get => availableNetworks;
            set
            {
                availableNetworks = value;
                OnPropertyChanged(nameof(AvailableNetworks));
            }
        }

        public Connman.WifiNetworkInfo? SelectedNetwork
        {
            get => selectedNetwork;
            set
            {
                selectedNetwork = value;
                OnPropertyChanged(nameof(SelectedNetwork));
            }
        }

        public string NetworkPassword
        {
            get => networkPassword;
            set
            {
                networkPassword = value;
                OnPropertyChanged(nameof(NetworkPassword));
            }
        }

        public ICommand ForgetSsidCommand { get; }
        public ICommand DisconnectSsidCommand { get; }
        public ICommand ConnectToNetworkCommand { get; }
        public ICommand ScanForNetworksCommand { get; }

        private async void ForgetSsid()
        {
            if (SelectedNetwork.HasValue && !IsDefault(SelectedNetwork.Value))
            {
                var network = SelectedNetwork.Value;
                bool result = Connman.ConnmanctlForgetSsid(connman, network);
                await Task.Delay(3500);
                ScanForNetworks();
            }
        }

        private void DisconnectSsid()
        {
            if (SelectedNetwork.HasValue)
            {
                var network = SelectedNetwork.Value;
                Connman.ConnmanctlDisconnectSsid(connman, network);
                ScanForNetworks();
            }
        }

        private async void ConnectToNetwork()
        {
            if (SelectedNetwork.HasValue && !IsDefault(SelectedNetwork.Value))
            {
                var network = SelectedNetwork.Value;
                network.Passphrase = NetworkPassword;
                try
                {
                    Connman.ConnmanctlConnectSsid(connman, network);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error connecting to network: {ex.Message}");
                }
                await Task.Delay(3500);
                ScanForNetworks();
            }
        }

        private bool IsDefault(Connman.WifiNetworkInfo netinfo)
        {
            return string.IsNullOrEmpty(netinfo.Ssid) && string.IsNullOrEmpty(netinfo.NetId);
        }

        private async void ScanForNetworks()
        {
            Connman.ConnmanctlScan(connman);
            await Task.Delay(3500);
            UpdateAvailableNetworks();
        }

        private void UpdateAvailableNetworks()
        {
            var networks = Connman.ConnmanctlGetSsids(connman)?.NetList;

            if (networks == null)
            {
                AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>();
            }
            else
            {
                AvailableNetworks = new ObservableCollection<Connman.WifiNetworkInfo>(
                    networks.Where(n => !string.IsNullOrEmpty(n.Ssid)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}