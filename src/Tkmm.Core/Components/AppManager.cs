using System.Diagnostics;
using System.IO.Compression;
using System.IO.Pipes;
using System.Text;
using Tkmm.Core.Helpers.Models;
using Tkmm.Core.Helpers.Operations;

namespace Tkmm.Core.Components;

public static class AppManager
{
    private static readonly string _appPath = Path.Combine(Config.Shared.StaticStorageFolder, "bin", "Tkmm.Desktop.exe");
    private static readonly string _appFolder = Path.Combine(Config.Shared.StaticStorageFolder, "bin");
    private static readonly string _appVersionFile = Path.Combine(Config.Shared.StaticStorageFolder, "version");

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

    public static async Task StartServerListener()
    {
        using NamedPipeServerStream server = new(ID);
        server.WaitForConnection();

        using (var reader = new BinaryReader(server, Encoding.UTF8)) {
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

    public static bool IsInstalled()
    {
        return File.Exists(_appVersionFile);
    }

    public static async Task<bool> HasUpdate()
    {
        if (!File.Exists(_appVersionFile)) {
            return true;
        }

        string currentVersion = File.ReadAllText(_appVersionFile);
        return await GitHubOperations.HasUpdate("TKMM-Team", "Tkmm", currentVersion);
    }

    public static async Task Update()
    {
        (Stream stream, string tag) = await GitHubOperations
            .GetLatestRelease("TKMM-Team", "Tkmm", Dependency.GetOSName());

        File.WriteAllText(_appVersionFile, tag);

        using ZipArchive archive = new(stream);
        archive.ExtractToDirectory(_appFolder);
    }
}