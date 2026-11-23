using System.Diagnostics;
using System.Reflection;
using System.Text;
using Tkmm.Core.Helpers;

namespace Tkmm.Core.Services;

public static class CrashHandler
{
    private const string CRASH_HANDLER_ARGUMENT = "--crash-handler";
    private const string CRASH_INFO_ARGUMENT = "--crash-info";
    private const string MUTEX_NAME = "TKMM-CrashHandler-[2E988D65-5221-4004-B282-E2B9E47A3AEF]";

    private static Mutex? _mutex;

    public static bool IsRunning()
    {
        try {
            using var mutex = Mutex.OpenExisting(MUTEX_NAME);
            return true;
        }
        catch (WaitHandleCannotBeOpenedException) {
            return false;
        }
        catch (IOException) {
            return false;
        }
    }

    public static string? TryGetCrashInfoPath(IReadOnlyList<string> args)
    {
        if (!args.Contains(CRASH_HANDLER_ARGUMENT)) {
            return null;
        }

        for (var i = 0; i < args.Count - 1; i++) {
            if (args[i] == CRASH_INFO_ARGUMENT) {
                return args[i + 1];
            }
        }

        return null;
    }

    public static bool IsLaunch(IReadOnlyList<string>? args = null)
    {
        args ??= [.. Environment.GetCommandLineArgs().Skip(1)];
        return TryGetCrashInfoPath(args) is not null;
    }

    private static Exception GetRootException(Exception exception)
    {
        while (true) {
            var next = exception switch {
                TypeInitializationException { InnerException: { } innerException } => innerException,
                TargetInvocationException { InnerException: { } innerException } => innerException,
                AggregateException { InnerExceptions.Count: 1 } aggregate => aggregate.InnerExceptions[0],
                _ => null
            };

            if (next is null) {
                return exception;
            }

            exception = next;
        }
    }

    public static bool TryRelaunchFatal(Exception exception)
    {
        return !IsLaunch() && TryRelaunch(GetRootException(exception));
    }

    public static bool TryAcquireForCurrentProcess()
    {
        if (_mutex is not null) {
            return true;
        }

        try {
            _mutex = new Mutex(initiallyOwned: true, MUTEX_NAME, out var createdNew);
            if (createdNew) {
                return true;
            }

            _mutex.Dispose();
            _mutex = null;
            return false;
        }
        catch (IOException) {
            _mutex = null;
            return true;
        }
    }

    public static void ReleaseForRestart()
    {
        if (_mutex is null) {
            return;
        }

        _mutex.ReleaseMutex();
        _mutex.Dispose();
        _mutex = null;
    }

    private static bool TryRelaunch(Exception exception)
    {
        if (IsRunning()) {
            return false;
        }

        var crashInfoPath = WriteCrashInfo(exception);
        if (crashInfoPath is null) {
            return false;
        }

        var sourcePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(sourcePath)) {
            return false;
        }

        SingleInstanceAppManager.MarkRestarting();

#if SWITCH
        var script =
            "systemctl stop tkmm; " +
            $"exec {BashQuote(sourcePath)} {CRASH_HANDLER_ARGUMENT} {CRASH_INFO_ARGUMENT} {BashQuote(crashInfoPath)}";

        Process.Start(new ProcessStartInfo {
            FileName = "/bin/bash",
            ArgumentList = { "-c", script },
            UseShellExecute = false
        });
#else
        Process.Start(new ProcessStartInfo(sourcePath) {
            UseShellExecute = true,
            ArgumentList = {
                CRASH_HANDLER_ARGUMENT,
                CRASH_INFO_ARGUMENT,
                crashInfoPath
            }
        });
#endif

        return true;
    }

#if SWITCH
    public static void StartTkmmService()
    {
        NxProcessHelper.Exec("systemctl start tkmm");
    }

    private static string BashQuote(string value)
    {
        return "'" + value.Replace("'", "'\\''") + "'";
    }
#endif

    private static string? WriteCrashInfo(Exception exception)
    {
        try {
            var crashFolder = Path.Combine(Path.GetTempPath(), "tkmm", "crashes");
            Directory.CreateDirectory(crashFolder);

            var crashInfoPath = Path.Combine(crashFolder, $"{Guid.NewGuid():N}.txt");
            File.WriteAllText(crashInfoPath, exception.ToString(), Encoding.UTF8);
            return crashInfoPath;
        }
        catch (Exception ex) when (ReadOnlyFilesystemHelper.IsReadOnlyFilesystemException(ex)) {
            return null;
        }
        catch (IOException) {
            return null;
        }
    }
}