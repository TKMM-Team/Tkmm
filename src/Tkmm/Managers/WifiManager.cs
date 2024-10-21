using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tkmm.Managers;

public class Connman
{
    public struct WifiNetworkInfo
    {
        public string Ssid { get; set; }
        public string NetId { get; set; }
        public bool Connected { get; set; }
        public bool SavedPassword { get; set; }
        public string Passphrase { get; set; }
    }

    private static string macAddress;
    private static string ipAddress;
    private static string netmask;
    private static string gateway;

    public static event Action NetworkDetailsChanged;

    public static string MacAddress
    {
        get => macAddress;
        private set
        {
            if (macAddress != value)
            {
                macAddress = value;
                NetworkDetailsChanged?.Invoke();
            }
        }
    }

    public static string IpAddress
    {
        get => ipAddress;
        private set
        {
            if (ipAddress != value)
            {
                ipAddress = value;
                NetworkDetailsChanged?.Invoke();
            }
        }
    }

    public static string Netmask
    {
        get => netmask;
        private set
        {
            if (netmask != value)
            {
                netmask = value;
                NetworkDetailsChanged?.Invoke();
            }
        }
    }

    public static string Gateway
    {
        get => gateway;
        private set
        {
            if (gateway != value)
            {
                gateway = value;
                NetworkDetailsChanged?.Invoke();
            }
        }
    }

    public static void RetrieveMacAddress()
    {
        using (var output = ExecuteCommand("ip link show wlan0"))
        {
            string result = output.ReadToEnd();
            var match = Regex.Match(result, @"link/ether (\S+)");
            MacAddress = match.Success ? match.Groups[1].Value.ToUpper() : string.Empty;
        }
    }

    public class ConnmanT
    {
        public WifiNetworkScan Scan;
        public string Command = new string(new char[300]);
        public bool ConnmanctlWidgetsSupported;
    }

    public class WifiNetworkScan
    {
        public WifiNetworkInfo[] NetList;
        public DateTime ScanTime;
    }

    public static ConnmanT ConnmanctlInit()
    {
        var connman = new ConnmanT
        {
            Scan = new WifiNetworkScan
            {
                NetList = Array.Empty<WifiNetworkInfo>()
            }
        };
        return connman;
    }

    private static bool IsDefault(WifiNetworkInfo netinfo)
    {
        return string.IsNullOrEmpty(netinfo.Ssid) && string.IsNullOrEmpty(netinfo.NetId) && !netinfo.Connected && !netinfo.SavedPassword && string.IsNullOrEmpty(netinfo.Passphrase);
    }

    public static void ConnmanctlFree(ConnmanT connman)
    {
        if (connman.Scan.NetList != null)
            connman.Scan.NetList = null;
    }

    public static bool ConnmanctlStart(ConnmanT connman)
    {
        return true;
    }

    public static void ConnmanctlStop(ConnmanT connman)
    {
    }

    public static void ConnmanctlRefreshServices(ConnmanT connman)
    {
        if (connman == null || connman.Scan == null)
        {
            Trace.WriteLine("Connman or Scan is null.");
            return;
        }
        RetrieveMacAddress();

        using (var servFile = ExecuteCommand("connmanctl services"))
        {
            if (connman.Scan.NetList != null)
                connman.Scan.NetList = null;

            bool isAnyNetworkConnected = false;

            string line;
            while ((line = servFile.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("wifi_"))
                    continue;

                var entry = new WifiNetworkInfo();
                var list = line.Substring(4).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                entry.Ssid = string.Join(" ", list, 0, list.Length - 1);
                entry.NetId = list[list.Length - 1];
                entry.Connected = line[2] == 'R' || line[2] == 'O';
                entry.SavedPassword = File.Exists(Path.Combine(CONNMAN_DIR, entry.NetId, "settings"));

                if (entry.Connected)
                {
                    GetNetworkDetails(entry.NetId);
                    isAnyNetworkConnected = true;
                }

                if (entry.NetId.StartsWith("wifi_"))
                {
                    var netList = new List<WifiNetworkInfo>(connman.Scan.NetList ?? Array.Empty<WifiNetworkInfo>());
                    netList.Add(entry);
                    connman.Scan.NetList = netList.ToArray();
                }
            }

            if (!isAnyNetworkConnected)
            {
                ResetNetworkDetails();
            }
        }
    }

    private static void GetNetworkDetails(string netId)
    {
        using (var detailFile = ExecuteCommand($"connmanctl services {netId}"))
        {
            string detailLine;
            while ((detailLine = detailFile.ReadLine()) != null)
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
    }

    private static string ExtractValue(string line, string key)
    {
        var startIndex = line.IndexOf(key + "=");
        if (startIndex == -1) return "N/A";

        startIndex += key.Length + 1;
        var endIndex = line.IndexOf(',', startIndex);
        if (endIndex == -1) endIndex = line.IndexOf(']', startIndex);

        var extractedValue = endIndex == -1 ? string.Empty : line.Substring(startIndex, endIndex - startIndex).Trim();
        return string.IsNullOrEmpty(extractedValue) ? "N/A" : extractedValue;
    }

    public static void CheckAndResetNetworkDetails(ConnmanT connman)
    {
        bool isAnyNetworkConnected = connman.Scan.NetList?.Any(network => network.Connected) ?? false;

        if (!isAnyNetworkConnected)
        {
            ResetNetworkDetails();
        }

        NetworkDetailsChanged?.Invoke();
    }

    public static void ResetNetworkDetails()
    {
        IpAddress = "N/A";
        Netmask = "N/A";
        Gateway = "N/A";
    }

    public static void ConnmanctlScan(ConnmanT connman)
    {
        ExecuteCommand("connmanctl scan wifi");
        connman.Scan.ScanTime = DateTime.Now;
        ConnmanctlRefreshServices(connman);
    }

    public static WifiNetworkScan ConnmanctlGetSsids(ConnmanT connman)
    {
        return connman.Scan;
    }

    public static bool ConnmanctlSsidIsOnline(ConnmanT connman, int i)
    {
        return connman.Scan.NetList != null && i < connman.Scan.NetList.Length && connman.Scan.NetList[i].Connected;
    }

    public static bool ConnmanctlConnectionInfo(ConnmanT connman, out WifiNetworkInfo netinfo)
    {
        netinfo = new WifiNetworkInfo();
        if (connman.Scan.NetList == null)
            return false;

        foreach (var net in connman.Scan.NetList)
        {
            if (net.Connected)
            {
                netinfo = net;
                return true;
            }
        }

        return false;
    }

    public static async Task<bool> ConnmanctlConnectSsidAsync(ConnmanT connman, WifiNetworkInfo netinfo)
    {
        var netid = netinfo.NetId;
        var settingsDir = Path.Combine(CONNMAN_DIR, netid);
        var settingsPath = Path.Combine(settingsDir, "settings");

        try
        {
            if (!File.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsDir);
                using (var settingsFile = new StreamWriter(settingsPath))
                {
                    settingsFile.WriteLine($"[{netid}]");
                    settingsFile.WriteLine($"Name={netinfo.Ssid}");
                    settingsFile.WriteLine("SSID=" + (BitConverter.ToString(Encoding.UTF8.GetBytes(netinfo.Ssid)).Replace("-", "")).ToLower());
                    settingsFile.WriteLine("Favorite=true");
                    settingsFile.WriteLine("AutoConnect=true");
                    settingsFile.WriteLine($"Passphrase={netinfo.Passphrase}");
                    settingsFile.WriteLine("IPv4.method=dhcp");
                }

                ExecuteCommand("systemctl restart connman.service");
                await Task.Delay(1000);
                ConnmanctlScan(connman);
                await Task.Delay(2000);
            }
            else { ExecuteCommand($"connmanctl connect {netinfo.NetId}"); }
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in ConnmanctlConnectSsid: {ex.Message}");
            return false;
        }
    }

    public static bool ConnmanctlDisconnectSsid(ConnmanT connman, WifiNetworkInfo netinfo)
    {
        ExecuteCommand($"connmanctl disconnect {netinfo.NetId}");
        ConnmanctlRefreshServices(connman);
        return true;
    }

    public static bool ConnmanctlForgetSsid(ConnmanT connman, WifiNetworkInfo netinfo)
    {
        var netid = netinfo.NetId;
        var settingsDir = Path.Combine(CONNMAN_DIR, netid);

        if (Directory.Exists(settingsDir))
        {
            Directory.Delete(settingsDir, true);
            ConnmanctlRefreshServices(connman);

            netinfo.SavedPassword = false;
        }
        
        UpdateNetworkList(connman, netid, network =>
        {
            network.SavedPassword = false;
            return network;
        });
        return true;
    }

    public static void ConnmanctlGetConnectedSsid(ConnmanT connman)
    {
        connman.Command = "connmanctl services | grep wifi_ | grep \"^..\\(R\\|O\\)\" | awk '{print $NF}'";
        using (var commandFile = ExecuteCommand(connman.Command))
        {
            var connectedNetId = commandFile.ReadLine()?.TrimEnd('\n');

            UpdateNetworkList(connman, connectedNetId, network =>
            {
                network.Connected = network.NetId == connectedNetId;
                return network;
            });
        }
    }

    private static void UpdateNetworkList(ConnmanT connman, string netId, Func<WifiNetworkInfo, WifiNetworkInfo> updateAction)
    {
        if (connman.Scan.NetList != null)
        {
            for (int i = 0; i < connman.Scan.NetList.Length; i++)
            {
                if (connman.Scan.NetList[i].NetId == netId)
                {
                    connman.Scan.NetList[i] = updateAction(connman.Scan.NetList[i]);
                    break;
                }
            }
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

    private const string CONNMAN_DIR = "/storage/.cache/connman/";
}