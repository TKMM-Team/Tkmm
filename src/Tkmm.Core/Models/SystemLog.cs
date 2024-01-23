using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.Models;

public partial class SystemLog : ObservableObject
{
    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private DateTime _date = DateTime.UtcNow;

    [ObservableProperty]
    private LogLevel _logLevel;

    public string ToMarkdown()
    {
        return $"""
            `[{LogLevel}]` `[{Date}]`

            ```
            {Message}
            ```
            """;
    }

    public SystemLog(string message)
    {
        _message = message;

        int startIndex = message.IndexOf('[');
        int endIndex = message.IndexOf(']');

        if (startIndex > -1 && endIndex > -1 && Enum.TryParse(message[++startIndex..endIndex], ignoreCase: false, out LogLevel logLevel)) {
            _logLevel = logLevel;
        }
    }

    public SystemLog(string message, LogLevel logLevel)
    {
        _message = message;
        _logLevel = logLevel;
    }
}
