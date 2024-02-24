using System.Diagnostics;

namespace Tkmm.Core;

public enum LogLevel
{
    None,
    Default,
    Info,
    Debug,
    Warning,
    Error
}

public static class AppLog
{
    static AppLog()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    public static void Log(string message, LogLevel level)
    {
        Trace.WriteLine(level == LogLevel.Default ? message : $"[{level}] {message}");
    }

    public static void Log(Exception ex)
    {
        Trace.WriteLine($"[{LogLevel.Error}] {ex}");
    }
}
