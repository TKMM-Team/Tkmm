using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Tkmm.IO.Desktop;

public class DesktopLogger : ILogger
{
    private const string GENERIC_USERNAME = "%username%";
    
    private static readonly string _userName = Path.GetFileName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    private static readonly string _targetLogFile = Path.Combine(AppContext.BaseDirectory, "log.txt");
    private readonly StreamWriter _writer;

    public DesktopLogger()
    {
        FileStream fs = File.Create(_targetLogFile);
        _writer = new StreamWriter(fs);
    }
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string log = $"""
            [{eventId.Id,2}: {logLevel,-12}]
                 TKMM - {formatter(state, exception).Replace(_userName, GENERIC_USERNAME)}
            """;
        
#if DEBUG
        Debug.WriteLine(log);
#endif
        
        _writer.WriteLine(log);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }
}