using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tkmm.Managers
{
    public partial class Connman
    {
        public struct WifiNetworkInfo
        {
            public string Ssid { get; set; }
            public string NetId { get; set; }
            public bool Connected { get; set; }
            public bool SavedPassword { get; set; }
            public string Passphrase { get; set; }
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
                if (_macAddress != value)
                {
                    _macAddress = value;
                    NetworkDetailsChanged?.Invoke();
                }
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

        private void RetrieveMacAddress()
        {
            using var output = ExecuteCommand("ip link show wlan0");
            var result = output.ReadToEnd();
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
            public WifiNetworkInfo[] NetList { get; set; } = [];
        }

        public static ConnmanT ConnmanctlInit()
        {
            return new ConnmanT
            {
                Scan = new WifiNetworkScan
                {
                    NetList = []
                }
            };
        }

        public void ConnmanctlRefreshServices(ConnmanT connman)
        {
            RetrieveMacAddress();

            using var servFile = ExecuteCommand("connmanctl services");
            connman.Scan.NetList = [];

            bool isAnyNetworkConnected = false;

            while (servFile.ReadLine() is { } line)
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
                    GetNetworkDetails(entry.NetId);
                    isAnyNetworkConnected = true;
                }

                if (entry.NetId.StartsWith("wifi_"))
                {
                    connman.Scan.NetList = connman.Scan.NetList.Append(entry).ToArray();
                }
            }

            if (!isAnyNetworkConnected)
            {
                ResetNetworkDetails();
            }
        }

        private void GetNetworkDetails(string netId)
        {
            using var detailFile = ExecuteCommand($"connmanctl services {netId}");
            while (detailFile.ReadLine() is { } detailLine)
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

        public void ConnmanctlScan(ConnmanT connman)
        {
            ExecuteCommand("connmanctl scan wifi");
            ConnmanctlRefreshServices(connman);
        }

        public static WifiNetworkScan ConnmanctlGetSsids(ConnmanT connman)
        {
            return connman.Scan;
        }

        public async Task ConnmanctlConnectSsidAsync(ConnmanT connman, WifiNetworkInfo netinfo)
        {
            var netid = netinfo.NetId;
            var settingsDir = Path.Combine(CONNMAN_DIR, netid);
            var settingsPath = Path.Combine(settingsDir, "settings");

            try
            {
                if (!File.Exists(settingsPath))
                {
                    Directory.CreateDirectory(settingsDir);
                    await using (var settingsFile = new StreamWriter(settingsPath))
                    {
                        await settingsFile.WriteLineAsync($"[{netid}]");
                        await settingsFile.WriteLineAsync($"Name={netinfo.Ssid}");
                        await settingsFile.WriteLineAsync($"SSID={BitConverter.ToString(Encoding.UTF8.GetBytes(netinfo.Ssid)).Replace("-", "").ToLowerInvariant()}");
                        await settingsFile.WriteLineAsync("Favorite=true");
                        await settingsFile.WriteLineAsync("AutoConnect=true");
                        await settingsFile.WriteLineAsync($"Passphrase={netinfo.Passphrase}");
                        await settingsFile.WriteLineAsync("IPv4.method=dhcp");
                    }

                    ExecuteCommand("systemctl restart connman.service");
                    await Task.Delay(1000);
                    ConnmanctlScan(connman);
                    await Task.Delay(2000);
                }
                else
                {
                    ExecuteCommand($"connmanctl connect {netinfo.NetId}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in ConnmanctlConnectSsid: {ex.Message}");
            }
        }

        public void ConnmanctlDisconnectSsid(ConnmanT connman, WifiNetworkInfo netinfo)
        {
            ExecuteCommand($"connmanctl disconnect {netinfo.NetId}");
            ConnmanctlRefreshServices(connman);
        }

        public static void ConnmanctlForgetSsid(ConnmanT connman, WifiNetworkInfo netinfo)
        {
            var settingsDir = Path.Combine(CONNMAN_DIR, netinfo.NetId);

            if (Directory.Exists(settingsDir))
            {
                Directory.Delete(settingsDir, true);
            }

            UpdateNetworkList(connman, netinfo.NetId, network =>
            {
                network.SavedPassword = false;
                return network;
            });
        }

        public static void ConnmanctlGetConnectedSsid(ConnmanT connman)
        {
            connman.Command = "connmanctl services | grep wifi_ | grep \"^..\\(R\\|O\\)\" | awk '{print $NF}'";
            using var commandFile = ExecuteCommand(connman.Command);
            var connectedNetId = commandFile.ReadLine()?.TrimEnd('\n');

            UpdateNetworkList(connman, connectedNetId, network =>
            {
                network.Connected = network.NetId == connectedNetId;
                network.SavedPassword = network.SavedPassword;
                return network;
            });
        }

        private static void UpdateNetworkList(ConnmanT connman, string? netId, Func<WifiNetworkInfo, WifiNetworkInfo> updateAction)
        {
            for (var i = 0; i < connman.Scan.NetList.Length; i++)
            {
                if (connman.Scan.NetList[i].NetId != netId) continue;

                connman.Scan.NetList[i] = updateAction(connman.Scan.NetList[i]);
                break;
            }
        }

        private static StreamReader ExecuteCommand(string command)
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
            return process.StandardOutput;
        }
    }
}