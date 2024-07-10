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
    private static readonly string _userName = Path.GetFileName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    static AppLog()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    public static void Log(string message, LogLevel level)
    {
        message = message.Replace(_userName, "%username%");
        Trace.WriteLine(level == LogLevel.Default
            ? message : $"[{level}] {message}");
    }

    public static void Log(Exception ex)
    {
        Trace.WriteLine($"[{LogLevel.Error}] {ex.ToString().Replace(_userName, "%username%")}");
    }
}
