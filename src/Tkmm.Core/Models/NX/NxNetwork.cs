using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Services;
using TkSharp.Core;

namespace Tkmm.Core.Models.NX;

public sealed partial class NxNetwork(string id, string ssid) : ObservableObject
{
    [ObservableProperty]
    private string _id = id;
    
    [ObservableProperty]
    private string _ssid = string.IsNullOrWhiteSpace(ssid) ? id : ssid;
    
    [ObservableProperty]
    private bool _isConnected;
    
    [ObservableProperty]
    private bool _isKnown;
    
    [ObservableProperty]
    private string _passphrase = string.Empty;
    
    [ObservableProperty]
    private string? _ipAddress;
    
    [ObservableProperty]
    private string? _subnetMask;
    
    [ObservableProperty]
    private string? _gateway;

    [RelayCommand]
    public async Task Connect(CancellationToken ct)
    {
        try {
            await Connman.Connect(this, ct);
            IsConnected = true;
            Passphrase = string.Empty;
        }
        catch (OperationCanceledException) {
            TkLog.Instance.LogInformation("The connection request to '{SSID}' was canceled.", Ssid);
        }
    }

    [RelayCommand]
    public async Task Disconnect(CancellationToken ct = default)
    {
        await Connman.Disconnect(this, ct);
        IsConnected = false;
    }

    [RelayCommand]
    public async Task Forget(CancellationToken ct = default)
    {
        await Connman.Disconnect(this, ct);
        await Connman.Forget(this, ct);
        IsConnected = false;
    }

    private void ClearProperties()
    {
        IpAddress = null;
        SubnetMask = null;
        Gateway = null;
    }
}