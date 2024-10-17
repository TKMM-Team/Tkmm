using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
            Console.WriteLine("Connman or Scan is null.");
            return;
        }

        using (var servFile = ExecuteCommand("connmanctl services"))
        {
            if (connman.Scan.NetList != null)
                connman.Scan.NetList = null;

            string line;
            while ((line = servFile.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("wifi_"))
                    continue;

                var entry = new WifiNetworkInfo();
                var list = line.Substring(4).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                entry.Connected = line[2] == 'R' || line[2] == 'O';
                entry.SavedPassword = line[0] == '*';

                entry.Ssid = string.Join(" ", list, 0, list.Length - 1);
                entry.NetId = list[list.Length - 1];

                if (entry.NetId.StartsWith("wifi_"))
                {
                    var netList = new List<WifiNetworkInfo>(connman.Scan.NetList ?? Array.Empty<WifiNetworkInfo>());
                    netList.Add(entry);
                    connman.Scan.NetList = netList.ToArray();
                }
            }
        }
    }

    public static bool ConnmanctlEnable(ConnmanT connman, bool enabled)
    {
        ExecuteCommand(enabled ? "connmanctl enable wifi" : "connmanctl disable wifi");
        ConnmanctlRefreshServices(connman);
        return true;
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

    public static bool ConnmanctlDisconnectSsid(ConnmanT connman, WifiNetworkInfo netinfo)
    {
        connman.Command = $"connmanctl disconnect {netinfo.NetId} 2>&1";
        ExecuteCommand(connman.Command);
        ConnmanctlRefreshServices(connman);
        return true;
    }

    public static bool ConnmanctlConnectSsid(ConnmanT connman, WifiNetworkInfo netinfo)
    {
        var netid = netinfo.NetId;
        var settingsDir = Path.Combine(CONNMAN_DIR, netid);
        var settingsPath = Path.Combine(settingsDir, "settings");

        Directory.CreateDirectory(settingsDir);

        if (!netinfo.SavedPassword)
        {
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
        }
        else if (!File.Exists(settingsPath))
        {
            ExecuteCommand("systemctl restart connman.service");
            return false;
        }

        connman.Command = $"connmanctl connect {netinfo.NetId}";
        ExecuteCommand(connman.Command);
        ConnmanctlRefreshServices(connman);

        for (int i = 0; i < connman.Scan.NetList.Length; i++)
        {
            var net = connman.Scan.NetList[i];
            if (net.NetId == netid)
            {
                if (!net.Connected)
                {
                    net.SavedPassword = false;
                    File.Delete(settingsPath);
                    connman.Scan.NetList[i] = net; // Update the modified net back to the array
                }
                return net.Connected;
            }
        }

        return false;
    }

    public static void ConnmanctlGetConnectedSsid(ConnmanT connman, StringBuilder ssid, int bufferSize)
    {
        if (bufferSize < 1) return;

        connman.Command = "connmanctl services | grep wifi_ | grep \"^..\\(R\\|O\\)\" | awk '{for (i=2; i<NF; i++) printf $i \" \"; print \"\"}'";
        using (var commandFile = ExecuteCommand(connman.Command))
        {
            ssid.Append(commandFile.ReadLine()?.TrimEnd('\n'));
        }
    }

    public static void ConnmanctlGetConnectedServiceName(ConnmanT connman, StringBuilder serviceName, int bufferSize)
    {
        if (bufferSize < 1) return;

        var temp = new StringBuilder(bufferSize);
        connman.Command = $"for serv in {CONNMAN_DIR}/wifi_*/ ; do if [ -d $serv ] ; then basename $serv ; fi ; done";

        using (var commandFile = ExecuteCommand(connman.Command))
        {
            string line;
            while ((line = commandFile.ReadLine()) != null)
            {
                temp.Clear().Append(line.TrimEnd('\n'));

                if (temp.Length == 0)
                {
                    continue;
                }

                connman.Command = $"connmanctl services {temp} | grep \"^  State = \\(online\\|ready\\)\" | wc -l";
                using (var serviceFile = ExecuteCommand(connman.Command))
                {
                    var ln = serviceFile.ReadLine()?.TrimEnd('\n');
                    if (ln == "1")
                    {
                        serviceName.Clear().Append(temp);
                        return;
                    }
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