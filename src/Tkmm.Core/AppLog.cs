using System.Diagnostics;

namespace Tkmm.Core;

public enum LogLevel
{
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
}
