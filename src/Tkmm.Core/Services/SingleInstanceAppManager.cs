using System.IO.Pipes;
using System.Text;

namespace Tkmm.Core.Services;

public static class SingleInstanceAppManager
{
    private const string ID = "TKMM-[2E988D65-5221-4004-B282-E2B9E47A3AEF]";
    
    private static Action<string[]>? _attach;
    
    public static bool Start(string[] args, Action<string[]> attach)
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
        foreach (string arg in args) {
            writer.Write(arg);
        }

        Console.WriteLine($"Redirecting [{args.Length}] to '{ID}'...");
        client.ReadByte();
        return false;
    }

    // ReSharper disable once FunctionRecursiveOnAllPaths
    private static async Task StartServerListener()
    {
        await using NamedPipeServerStream server = new(ID);
        await server.WaitForConnectionAsync();

        using (var reader = new BinaryReader(server, Encoding.UTF8)) {
            int argc = reader.ReadInt32();
            string[] args = new string[argc];
            for (int i = 0; i < argc; i++) {
                args[i] = reader.ReadString();
            }

            _attach?.Invoke(args);
            server.WriteByte(0);
        }

        await StartServerListener();
    }
}