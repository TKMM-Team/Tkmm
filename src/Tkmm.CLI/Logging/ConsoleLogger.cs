using Kokuban;
using Kokuban.AnsiEscape;
using Microsoft.Extensions.Logging;

namespace Tkmm.CLI.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        AnsiStyle style = logLevel switch {
            LogLevel.Information => Chalk.Rgb(46, 95, 201),
            LogLevel.Debug => Chalk.Rgb(148, 119, 237),
            LogLevel.Warning => Chalk.Rgb(0xC9, 0x96, 0x2E),
            LogLevel.Error => Chalk.Rgb(0xC9, 0x40, 0x2E),
            _ => default
        };
        
        Console.WriteLine(style + formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}