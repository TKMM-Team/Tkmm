using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tkmm.Managers;

namespace Tkmm.ViewModels.Pages;

public partial class NetworkSettingsPageViewModel : ObservableObject
{
    private readonly Connman _connman;

    [ObservableProperty]
    private ObservableCollection<string> availableNetworks = new();

    [ObservableProperty]
    private string? selectedNetwork;

    [ObservableProperty]
    private string? networkPassword;

    public NetworkSettingsPageViewModel()
    {
        _connman = new Connman();
        LoadNetworks();
    }

    private void LoadNetworks()
    {
        try
        {
            AvailableNetworks.Clear();
            var connmanInstance = Connman.ConnmanctlInit();
            Connman.ConnmanctlRefreshServices(connmanInstance);

            if (connmanInstance.Scan?.NetList != null)
            {
                foreach (var network in connmanInstance.Scan.NetList)
                {
                    AvailableNetworks.Add(network.Ssid);
                    Console.WriteLine($"Network found: {network.Ssid}");
                }
            }
            else
            {
                Console.WriteLine("No networks found or Scan is null.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading networks: " + ex.Message);
        }
    }

    [RelayCommand]
    private void ConnectToNetwork()
    {
        if (SelectedNetwork is null) return;

        try
        {
            var connmanInstance = Connman.ConnmanctlInit();
            var networkInfo = connmanInstance.Scan.NetList.FirstOrDefault(n => n.Ssid == SelectedNetwork);
            if (!string.IsNullOrEmpty(networkInfo.NetId))
            {
                networkInfo.Passphrase = NetworkPassword;
                Connman.ConnmanctlConnectSsid(connmanInstance, networkInfo);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error connecting to network: " + ex.Message);
        }
    }

    [RelayCommand]
    private void ScanForNetworks()
    {
        try
        {
            var connmanInstance = Connman.ConnmanctlInit();
            Connman.ConnmanctlScan(connmanInstance);
            LoadNetworks();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error scanning for networks: " + ex.Message);
        }
    }
}