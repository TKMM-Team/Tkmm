using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tkmm.Managers;

namespace Tkmm.ViewModels.Pages;

public partial class NetworkSettingsPageViewModel : ObservableObject
{
    private readonly Connman _connman;
    
    [ObservableProperty]
    private ObservableCollection<string> _availableNetworks = new();

    [ObservableProperty]
    private string? _selectedNetwork;

    public NetworkSettingsPageViewModel()
    {
        _connman = new Connman();
        LoadNetworks();
    }

    private void LoadNetworks()
    {
        // Simulate loading networks
        _availableNetworks.Clear();
        var connmanInstance = Connman.ConnmanctlInit();
        Connman.ConnmanctlRefreshServices(connmanInstance);
        foreach (var network in connmanInstance.Scan.NetList)
        {
            _availableNetworks.Add(network.Ssid);
        }
    }

    [RelayCommand]
    private void ConnectToNetwork()
    {
        if (_selectedNetwork is null) return;

        var connmanInstance = Connman.ConnmanctlInit();
        var networkInfo = connmanInstance.Scan.NetList.FirstOrDefault(n => n.Ssid == _selectedNetwork);

        if (!string.IsNullOrEmpty(networkInfo.NetId))
        {
            Connman.ConnmanctlConnectSsid(connmanInstance, networkInfo);
        }
    }
}