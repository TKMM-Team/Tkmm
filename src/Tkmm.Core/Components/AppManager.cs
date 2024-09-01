using System.Diagnostics;
using System.IO.Compression;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using Tkmm.Core.Helpers.Models;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Components;

public static class AppManager
{
    private const string ORG = "TKMM-Team";
    private const string REPO = "Tkmm";
    private const string APP_NAME = "TKMM";
    private const string PROC_NAME = "tkmm";
    private const string LAUNCHER_NAME = "TKMM Launcher";

    private static readonly string _ext = OperatingSystem.IsWindows() ? ".exe" : string.Empty;
    private static readonly string _platform =
        OperatingSystem.IsWindows() ? "win" :
        OperatingSystem.IsLinux() ? "linux" :
        OperatingSystem.IsMacOS() ? "osx" :
            throw new PlatformNotSupportedException(
                "Unsupported platform. Only Windows, Linux and MacOS are supported."
            );
    private static readonly string _rid = $"{_platform}-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}";

    private static readonly string _appFolder = Path.Combine(Config.Shared.StaticStorageFolder, "bin");
    private static readonly string _appPath = Path.Combine(_appFolder, $"tkmm{_ext}");
    private static readonly string _appVersionFile = Path.Combine(Config.Shared.StaticStorageFolder, "version");

    private static readonly string _launcherFolder = Path.Combine(Config.Shared.StaticStorageFolder, "launcher");
    private static readonly string _launcherPath = Path.Combine(_launcherFolder, $"tkmm-launcher{_ext}");

    private const string ID = "Tkmm-[9fcf39df-ec9a-4510-8f56-88b52e85ae01]";
    private static Func<string[], Task>? _attach;

    public static bool Start(string[] args, Func<string[], Task> attach)
    {
        _attach = attach;

        using NamedPipeClientStream client = new(ID);
        try {
            client.Connect(0);
        }
        catch {
            Task.Run(StartServerListener);
            return true;
        }

        using BinaryWriter writer = new(client, Encoding.UTF8);

        writer.Write(args.Length);
        for (int i = 0; i < args.Length; i++) {
            writer.Write(args[i]);
        }

        Console.WriteLine($"[Info] Waiting for '{ID}'...");
        client.ReadByte();
        return false;
    }

    // ReSharper disable once FunctionRecursiveOnAllPaths
    public static async Task StartServerListener()
    {
        await using NamedPipeServerStream server = new(ID);
        await server.WaitForConnectionAsync();

        using (BinaryReader reader = new(server, Encoding.UTF8)) {
            int argc = reader.ReadInt32();
            string[] args = new string[argc];
            for (int i = 0; i < argc; i++) {
                args[i] = reader.ReadString();
            }

            if (_attach?.Invoke(args) is Task task) {
                await task;
            }

            server.WriteByte(0);
        }

        await StartServerListener();
    }

    public static void Start()
    {
        Process.Start(_appPath);
    }

    public static void StartLauncher()
    {
        Process.Start(new ProcessStartInfo {
            FileName = _launcherPath,
            UseShellExecute = true,
        });
    }

    public static bool IsInstalled()
    {
        return File.Exists(_appVersionFile);
    }

    public static async Task<(bool Result, string Tag)> HasUpdate()
    {
        if (!File.Exists(_appVersionFile)) {
            return (true, "Latest");
        }

        string currentVersion = File.ReadAllText(_appVersionFile);
        return await GitHubOperations.HasUpdate(ORG, REPO, currentVersion);
    }

    public static async Task Update(Action<int> setProgress)
    {
        AppStatus.Set("Closing open app instances", "fa-solid fa-download");
        Kill();
        setProgress(20);

        AppStatus.Set("Downloading app", "fa-solid fa-download");
        (Stream stream, string tag) = await GitHubOperations
            .GetLatestRelease(ORG, REPO, assetName: $"TKMM-{_rid}.zip");
        setProgress(40);

        AppStatus.Set("Extracting release", "fa-solid fa-file-zipper");
        using ZipArchive archive = new(stream);
        archive.ExtractToDirectory(_appFolder, overwriteFiles: true);
        setProgress(60);

        AppStatus.Set("Updating version", "fa-solid fa-code-commit");
        File.WriteAllText(_appVersionFile, tag);
        setProgress(80);

        AppStatus.Set("Application installed!", "fa-solid fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
    }

    public static async Task UpdateLauncher()
    {
        AppStatus.Set("Downloading launcher", "fa-solid fa-download");

        (Stream stream, _) = await GitHubOperations
            .GetLatestRelease(ORG, REPO, assetName: $"TKMM-Launcher-{_rid}.zip");

        AppStatus.Set("Extracting release", "fa-solid fa-file-zipper");
        using ZipArchive archive = new(stream);
        archive.ExtractToDirectory(_launcherFolder, true);

        AppStatus.Set("Launcher updated!", "fa-solid fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
    }

    public static async Task Uninstall(Action<int> setProgress)
    {
        AppStatus.Set("Closing open app instances", "fa-solid fa-cicle-xmark");
        Kill();
        setProgress(20);

        AppStatus.Set("Uninstalling", "fa-solid fa-broom", isWorkingStatus: true);
        await Task.Delay(1000);
        setProgress(40);

        if (Directory.Exists(_appFolder)) {
            Directory.Delete(_appFolder, true);
        }
        setProgress(60);

        if (File.Exists(_appVersionFile)) {
            File.Delete(_appVersionFile);
        }
        setProgress(80);

        DeleteDesktopShortcuts();
        setProgress(100);
        AppStatus.Set("Uninstall Successful", "fa-regular fa-circle-check", isWorkingStatus: false);
    }

    private static readonly char _pathSeperatorChar = OperatingSystem.IsWindows() ? ';' : ':';
    public static void AddToPath()
    {
        const string pathName = "PATH";
        string path = Environment.GetEnvironmentVariable(pathName, EnvironmentVariableTarget.User)
            ?? string.Empty;

        if (path.Contains(_appFolder)) {
            return;
        }

        path += $"{_pathSeperatorChar}{_appFolder}";
        Environment.SetEnvironmentVariable(pathName, path, EnvironmentVariableTarget.User);
    }

    public static void CreateProtocol()
    {
        WebProtocol.Create(PROC_NAME, _appPath);
    }

    public static void CreateFileAssociation()
    {
        WindowsFileAssociation.Create(PROC_NAME, ".tkcl", _appPath);
    }

    public static void CreateDesktopShortcuts()
    {
        Shortcut.Create(APP_NAME, Location.Application, _appPath, "tkmm");
        Shortcut.Create(LAUNCHER_NAME, Location.Application, _launcherPath, "tkmm");
        Shortcut.Create(APP_NAME, Location.Desktop, _appPath, "tkmm");
    }

    public static void DeleteDesktopShortcuts()
    {
        Shortcut.Remove(APP_NAME, Location.Application);
        Shortcut.Remove(LAUNCHER_NAME, Location.Application);
        Shortcut.Remove(APP_NAME, Location.Desktop);
    }

    private static void Kill()
    {
        foreach (Process process in Process.GetProcessesByName(PROC_NAME)) {
            process.Kill();
        }
    }
}