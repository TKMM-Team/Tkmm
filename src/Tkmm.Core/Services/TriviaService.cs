namespace Tkmm.Core.Services;

public class TriviaService
{
    private static readonly string _triviaConfigPath = Path.Combine(Config.Shared.StaticStorageFolder, "trivia.txt");
    private static readonly string[]? _triviaMessages;
    private static readonly List<string> _unusedMessages = [];
    private static readonly Random _random = new();
    private static readonly Timer? _timer;

    public static bool IsWorking { get; set; } = false;

    static TriviaService()
    {
        if (!File.Exists(_triviaConfigPath)) {
            AppLog.Log("Trivia config could not be found, random merging messages will not be displayed", LogLevel.Warning);
            return;
        }

        _triviaMessages = File.ReadAllLines(_triviaConfigPath);
        _timer = new((e) => {
            if (IsWorking) {
                if (_unusedMessages.Count <= 0) {
                    _unusedMessages.AddRange(_triviaMessages);
                }

                int index = _random.Next(_unusedMessages.Count - 1);
                if (index >= _unusedMessages.Count || index < 0) {
                    return;
                }

                string message = _unusedMessages[index];
                _unusedMessages.RemoveAt(index);

                AppStatus.Set(message, "fa-solid fa-code-merge");
            }
        }, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(3));
    }

    public static void Start()
    {
        if (_timer is null || _triviaMessages is null) {
            return;
        }

        IsWorking = true;
    }

    public static void Stop()
    {
        IsWorking = false;
    }
}
