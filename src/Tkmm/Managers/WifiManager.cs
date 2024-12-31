using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tkmm.Managers
{
    public partial class Connman
    {
        private readonly object _netListLock = new();

        public class WifiNetworkInfo : INotifyPropertyChanged
        {
            private string _ssid;
            private string _netId;
            private bool _connected;
            private bool _savedPassword;
            private string _passphrase;

            public WifiNetworkInfo()
            {
                _ssid = string.Empty;
                _netId = string.Empty;
                _passphrase = string.Empty;
            }

            public string Ssid
            {
                get => _ssid;
                set
                {
                    if (_ssid != value)
                    {
                        _ssid = value;
                        OnPropertyChanged(nameof(Ssid));
                    }
                }
            }

            public string NetId
            {
                get => _netId;
                set
                {
                    if (_netId != value)
                    {
                        _netId = value;
                        OnPropertyChanged(nameof(NetId));
                    }
                }
            }

            public bool Connected
            {
                get => _connected;
                set
                {
                    if (_connected != value)
                    {
                        _connected = value;
                        OnPropertyChanged(nameof(Connected));
                    }
                }
            }

            public bool SavedPassword
            {
                get => _savedPassword;
                set
                {
                    if (_savedPassword != value)
                    {
                        _savedPassword = value;
                        OnPropertyChanged(nameof(SavedPassword));
                    }
                }
            }

            public string Passphrase
            {
                get => _passphrase;
                set
                {
                    if (_passphrase != value)
                    {
                        _passphrase = value;
                        OnPropertyChanged(nameof(Passphrase));
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public void NotifyPropertyChanged(string propertyName)
            {
                OnPropertyChanged(propertyName);
            }

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static readonly string[] _splitChars = [" "];
        private string? _macAddress;
        private string? _ipAddress;
        private string? _netmask;
        private string? _gateway;
        private const string CONNMAN_DIR = "/storage/.cache/connman/";

        public event Action? NetworkDetailsChanged;

        public string? MacAddress
        {
            get => _macAddress;
            private set
            {
                if (_macAddress == null && value != null) _macAddress = value;
            }
        }

        public string? IpAddress
        {
            get => _ipAddress;
            private set
            {
                if (_ipAddress != value)
                {
                    _ipAddress = value;
                    NetworkDetailsChanged?.Invoke();
                }
            }
        }

        public string? Netmask
        {
            get => _netmask;
            private set
            {
                if (_netmask != value)
                {
                    _netmask = value;
                    NetworkDetailsChanged?.Invoke();
                }
            }
        }

        public string? Gateway
        {
            get => _gateway;
            private set
            {
                if (_gateway != value)
                {
                    _gateway = value;
                    NetworkDetailsChanged?.Invoke();
                }
            }
        }

        [GeneratedRegex("link/ether (\\S+)")]
        private static partial Regex MacAddressRegex();

        private async Task RetrieveMacAddressAsync()
        {
            using var output = await ExecuteCommand("ip link show wlan0", true);
            var result = await output.ReadToEndAsync();
            var match = MacAddressRegex().Match(result);
            MacAddress = match.Success ? match.Groups[1].Value.ToUpper() : string.Empty;
        }

        public class ConnmanT
        {
            public WifiNetworkScan Scan { get; init; } = new();
            public string Command { get; set; } = new(new char[300]);
        }

        public class WifiNetworkScan
        {
            public WifiNetworkInfo[] NetList { get; set; } = Array.Empty<WifiNetworkInfo>();
        }

        public static ConnmanT ConnmanctlInit()
        {
            return new ConnmanT
            {
                Scan = new WifiNetworkScan
                {
                    NetList = Array.Empty<WifiNetworkInfo>()
                }
            };
        }

        public async Task ConnmanctlRefreshServicesAsync(ConnmanT connman)
        {
            await RetrieveMacAddressAsync();

            using var servFile = await ExecuteCommand("connmanctl services", true);
            var newNetList = new List<WifiNetworkInfo>();

            bool isAnyNetworkConnected = false;

            while (await servFile.ReadLineAsync() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("wifi_"))
                    continue;

                var entry = new WifiNetworkInfo();
                var list = line.AsSpan(4).ToString().Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);

                entry.Ssid = string.Join(" ", list, 0, list.Length - 1);
                entry.NetId = list[^1];
                entry.Connected = line[2] == 'R' || line[2] == 'O';
                entry.SavedPassword = File.Exists(Path.Combine(CONNMAN_DIR, entry.NetId, "settings"));

                if (entry.Connected)
                {
                    await GetNetworkDetailsAsync(entry.NetId);
                    isAnyNetworkConnected = true;
                }

                if (entry.NetId.StartsWith("wifi_")) newNetList.Add(entry);
            }

            lock (_netListLock) connman.Scan.NetList = newNetList.ToArray();
            if (!isAnyNetworkConnected) ResetNetworkDetails();
        }

        private async Task GetNetworkDetailsAsync(string netId)
        {
            using var detailFile = await ExecuteCommand($"connmanctl services {netId}", true);
            while (await detailFile.ReadLineAsync() is { } detailLine)
            {
                if (detailLine.Contains("Ethernet ="))
                {
                    MacAddress = ExtractValue(detailLine, "Address");
                }
                else if (detailLine.Contains("IPv4 ="))
                {
                    IpAddress = ExtractValue(detailLine, "Address");
                    Netmask = ExtractValue(detailLine, "Netmask");
                    Gateway = ExtractValue(detailLine, "Gateway");
                }
            }
        }

        private static string ExtractValue(string line, string key)
        {
            var startIndex = line.IndexOf(key + "=", StringComparison.Ordinal);
            if (startIndex == -1) return "N/A";

            startIndex += key.Length + 1;
            var endIndex = line.IndexOf(',', startIndex);
            if (endIndex == -1) endIndex = line.IndexOf(']', startIndex);

            var extractedValue = endIndex == -1 ? string.Empty : line[startIndex..endIndex].Trim();
            return string.IsNullOrEmpty(extractedValue) ? "N/A" : extractedValue;
        }

        private void ResetNetworkDetails()
        {
            IpAddress = "N/A";
            Netmask = "N/A";
            Gateway = "N/A";
        }

        public async Task ConnmanctlScanAsync(ConnmanT connman)
        {
            await Task.Run(() => ExecuteCommand("connmanctl scan wifi", true));
            await Task.Run(() => ConnmanctlRefreshServicesAsync(connman));
        }

        public async Task ConnmanctlRestartAsync(ConnmanT connman)
        {
            await Task.Run(() => ExecuteCommand("systemctl restart connman.service", true));
            await Task.Run(() => ConnmanctlScanAsync(connman));
        }

        public static WifiNetworkScan ConnmanctlGetSsids(ConnmanT connman)
        {
            return connman.Scan;
        }

        public async Task CreateNetworkConfigAsync(WifiNetworkInfo netinfo, ConnmanT connman)
        {
            var netid = netinfo.NetId;
            var settingsDir = Path.Combine(CONNMAN_DIR, netid);
            var settingsPath = Path.Combine(settingsDir, "settings");

            if (!File.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsDir);
                using (var settingsFile = new StreamWriter(settingsPath))
                {
                    await settingsFile.WriteLineAsync($"[{netid}]");
                    await settingsFile.WriteLineAsync($"Name={netinfo.Ssid}");
                    await settingsFile.WriteLineAsync($"SSID={BitConverter.ToString(Encoding.UTF8.GetBytes(netinfo.Ssid)).Replace("-", "").ToLowerInvariant()}");
                    await settingsFile.WriteLineAsync("Favorite=true");
                    await settingsFile.WriteLineAsync("AutoConnect=true");
                    await settingsFile.WriteLineAsync($"Passphrase={netinfo.Passphrase}");
                    await settingsFile.WriteLineAsync("IPv4.method=dhcp");
                }
            }

            await Task.Run(() => ConnmanctlRestartAsync(connman));
        }

        public void ConnmanctlConnectSsid(ConnmanT connman, WifiNetworkInfo netinfo)
        {
            var netid = netinfo.NetId;
            _ = Task.Run(() => ExecuteCommand($"connmanctl connect {netinfo.NetId}", true));
        }

        public void ConnmanctlDisconnectSsid(ConnmanT connman, WifiNetworkInfo netinfo)
        {
            Task.Run(async () => await ExecuteCommand($"connmanctl disconnect {netinfo.NetId}", true));
        }

        public static void ConnmanctlForgetSsid(ConnmanT connman, WifiNetworkInfo netinfo)
        {
            var settingsDir = Path.Combine(CONNMAN_DIR, netinfo.NetId);
            if (Directory.Exists(settingsDir)) Directory.Delete(settingsDir, true);

            UpdateNetworkList(connman, netinfo.NetId, network =>
            {
                network.SavedPassword = false;
                return network;
            });
        }

        public async Task ConnmanctlGetConnectedSsidAsync(ConnmanT connman)
        {
            connman.Command = "connmanctl services | grep wifi_ | grep \"^\\*A[OR]\" | awk '{print $NF}'";
            using var commandFile = await ExecuteCommand(connman.Command, true);
            var connectedNetId = (await commandFile.ReadLineAsync())?.TrimEnd('\n');

            UpdateNetworkList(connman, connectedNetId, network =>
            {
                network.Connected = network.NetId == connectedNetId;
                network.SavedPassword = network.SavedPassword;
                return network;
            });
        }

        private static void UpdateNetworkList(ConnmanT connman, string? netId, Func<WifiNetworkInfo, WifiNetworkInfo> updateAction)
        {
            lock (connman.Scan)
            {
                for (var i = 0; i < connman.Scan.NetList.Length; i++)
                {
                    if (connman.Scan.NetList[i].NetId != netId) continue;

                    connman.Scan.NetList[i] = updateAction(connman.Scan.NetList[i]);
                    break;
                }
            }
        }

        private static async Task<StreamReader> ExecuteCommand(string command, bool isAsync = false)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("")));
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            if (isAsync) await process.WaitForExitAsync();
            else process.WaitForExit();
            return process.StandardOutput;
        }
    }
}