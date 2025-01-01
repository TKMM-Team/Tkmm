using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Core.Models.NX;
using Tkmm.Core.Services;

namespace Tkmm.ViewModels.Pages;

public partial class NetworkSettingsPageViewModel : ObservableObject
{
    public List<NxNetworkService> NetworkServices { get; }

    public NxNetworkService WiFiService { get; }

    public ObservableCollection<NxNetwork> Networks { get; } = [];

    [ObservableProperty]
    private NxNetwork? _connected;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _macAddress;

    public NetworkSettingsPageViewModel()
    {
        WiFiService = new NxNetworkService(SystemMsg.NetworkSettings_WiFiService_Name, OnWiFiEnabledChanged) {
            IsEnabled = Connman.IsTechnologyEnabled("wifi")
        };

        NetworkServices = [
            WiFiService,
            new NxNetworkService(SystemMsg.NetworkSettings_SshService_Name, OnSshEnabledChanged) {
                IsEnabled = NxServices.IsSshEnabled()
            },
            new NxNetworkService(SystemMsg.NetworkSettings_SmbService_Name, OnSmbEnabledChanged) {
                IsEnabled = NxServices.IsSmbEnabled()
            }
        ];

        _macAddress = Connman.GetMacAddress();
    }

    [RelayCommand]
    private async Task LocateNetworks()
    {
        Networks.Clear();
        IsLoading = true;

        await Task.Run(async () => {
            await foreach (NxNetwork network in Connman.GetNetworks()) {
                Networks.Add(network);

                if (network.IsConnected) {
                    Connected = network;
                    _ = Task.Run(async () => await Connman.LoadNetworkProperties(network));
                }
            }
        });

        IsLoading = false;
    }

    private void OnWiFiEnabledChanged(bool isEnabled)
    {
        Connman.UpdateTechnology("wifi", isEnabled);

        if (isEnabled) {
            _ = Task.Run(async () => await LocateNetworks());
        }
        else {
            Networks.Clear();
        }
    }

    private static void OnSshEnabledChanged(bool isEnabled)
    {
        if (isEnabled) {
            NxServices.EnableSsh();
            return;
        }

        NxServices.DisableSsh();
    }

    private static void OnSmbEnabledChanged(bool isEnabled)
    {
        if (isEnabled) {
            NxServices.EnableSmb();
            return;
        }

        NxServices.DisableSmb();
    }
}