using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tkmm.Managers
{
    public class NetworkServices
    {
        private const string SshConfigPath = "/storage/.cache/services/sshd.conf";
        private const string SmbConfigPath = "/storage/.cache/services/samba.conf";

        public void EnableSSH()
        {
            File.Create(SshConfigPath).Dispose();
            ExecuteCommand("systemctl restart sshd");
        }

        public void DisableSSH()
        {
            if (File.Exists(SshConfigPath))
            {
                File.Delete(SshConfigPath);
            }
            ExecuteCommand("systemctl restart sshd");
        }

        public bool IsSSHEnabled()
        {
            return File.Exists(SshConfigPath);
        }

        public void EnableSMB()
        {
            File.Create(SmbConfigPath).Dispose();
            ExecuteCommand("systemctl restart smbd");
        }

        public void DisableSMB()
        {
            if (File.Exists(SmbConfigPath))
            {
                File.Delete(SmbConfigPath);
            }
            ExecuteCommand("systemctl stop smbd");
        }

        public bool IsSMBEnabled()
        {
            return File.Exists(SmbConfigPath);
        }

        public void EnableWiFi()
        {
            if (!IsWiFiEnabled())
            {
                ExecuteCommand("connmanctl enable wifi");
            }
        }

        public void DisableWiFi()
        {
            if (IsWiFiEnabled())
            {
                ExecuteCommand("connmanctl disable wifi");
            }
        }

        public bool IsWiFiEnabled()
        {
            var outputReader = ExecuteCommand("connmanctl technologies");
            var output = outputReader?.ReadToEnd() ?? string.Empty;

            bool isEnabled = output.Split(new[] { "/net/connman/technology/" }, StringSplitOptions.None)
                                    .Any(section => section.TrimStart().StartsWith("wifi", StringComparison.OrdinalIgnoreCase) &&
                                                    section.Contains("Powered = True", StringComparison.OrdinalIgnoreCase));
            return isEnabled;
        }

        private StreamReader ExecuteCommand(string command)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
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