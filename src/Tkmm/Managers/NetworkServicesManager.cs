using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tkmm.Managers
{
    public static class NetworkServices
    {
        private const string SSH_CONFIG_PATH = "/storage/.cache/services/sshd.conf";
        private const string SMB_CONFIG_PATH = "/storage/.cache/services/samba.conf";
        private const string COMMAND_RESTART_SSH = "systemctl restart sshd";
        private const string COMMAND_STOP_SMB = "systemctl stop smbd";
        private const string COMMAND_RESTART_SMB = "systemctl restart smbd";
        private const string COMMAND_ENABLE_WIFI = "connmanctl enable wifi";
        private const string COMMAND_DISABLE_WIFI = "connmanctl disable wifi";
        private const string COMMAND_CHECK_WIFI = "connmanctl technologies";

        public static void EnableSsh()
        {
            CreateAndDisposeFile(SSH_CONFIG_PATH);
            ExecuteCommand(COMMAND_RESTART_SSH);
        }

        public static void DisableSsh()
        {
            DeleteFileIfExists(SSH_CONFIG_PATH);
            ExecuteCommand(COMMAND_RESTART_SSH);
        }

        public static bool IsSshEnabled()
        {
            return File.Exists(SSH_CONFIG_PATH);
        }

        public static void EnableSmb()
        {
            CreateAndDisposeFile(SMB_CONFIG_PATH);
            ExecuteCommand(COMMAND_RESTART_SMB);
        }

        public static void DisableSmb()
        {
            DeleteFileIfExists(SMB_CONFIG_PATH);
            ExecuteCommand(COMMAND_STOP_SMB);
        }

        public static bool IsSmbEnabled()
        {
            return File.Exists(SMB_CONFIG_PATH);
        }

        public static void EnableWifi()
        {
            if (!IsWifiEnabled())
            {
                ExecuteCommand(COMMAND_ENABLE_WIFI);
            }
        }

        public static void DisableWifi()
        {
            if (IsWifiEnabled())
            {
                ExecuteCommand(COMMAND_DISABLE_WIFI);
            }
        }

        public static bool IsWifiEnabled()
        {
            var outputReader = ExecuteCommand(COMMAND_CHECK_WIFI);
            var output = outputReader?.ReadToEnd() ?? string.Empty;

            return output.Split(new[] { "/net/connman/technology/" }, StringSplitOptions.None)
                .Any(section => section.TrimStart().StartsWith("wifi", StringComparison.OrdinalIgnoreCase) &&
                                section.Contains("Powered = True", StringComparison.OrdinalIgnoreCase));
        }

        private static void CreateAndDisposeFile(string path)
        {
            File.Create(path).Dispose();
        }

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static StreamReader? ExecuteCommand(string command)
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