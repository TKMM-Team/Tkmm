global using Tkmm.Localization;
global using static Tkmm.Localization.LocaleManager;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;

namespace Tkmm.Localization;

public sealed class LocaleManager : ObservableObject
{
    private const string DEFAULT_LOCALE = "en_US";

    private readonly Dictionary<string, LocalesEntry> _entries;
    private string _currentCulture;

    public static LocaleManager Locale { get; } = Load();

    public static LocaleManager Load()
    {
        using var embeddedStream = typeof(LocaleManager).Assembly
            .GetManifestResourceStream("Tkmm.Resources.locales.json") ?? throw new NullReferenceException("Locale is not embedded.");

        var json = JsonSerializer.Deserialize(embeddedStream, LocalesJsonContext.Default.LocalesJson);
        return new LocaleManager(json);
    }

    private LocaleManager(LocalesJson json)
    {
        Languages = json.Languages;
        _entries = json.Locales;
        _currentCulture = Config.Shared.CultureName.Value.Replace('-', '_');

        Config.Shared.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(Config.Shared.CultureName)) {
                _currentCulture = Config.Shared.CultureName.Value.Replace('-', '_');
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("Translation");
            }
        };
    }

    public string this[TkLocale key] => this[Enum.GetName(key)!];

    public string this[TkLocale key, params object[] arguments] => string.Format(this[key], arguments);

    public string this[string key] => this[key, failSoftly: false, culture: null];

    public string this[string key, bool failSoftly] => this[key, failSoftly, culture: null];

    public string this[string key, bool failSoftly, string? culture] {
        get {
            if (!_entries.TryGetValue(key, out var entry)) {
                if (failSoftly) {
                    return key;
                }
                
                throw new ArgumentException(
                    $"The locale entry '{key}' does not exist.");
            }

            culture ??= _currentCulture;
            if (!entry.TryGetValue(culture, out string? value)) {
                throw new ArgumentException(
                    $"The locale entry '{key}' is missing a translation entry for '{culture}'.");
            }

            return string.IsNullOrWhiteSpace(value)
                ? entry.GetValueOrDefault(DEFAULT_LOCALE)
                  ?? throw new ArgumentException($"The locale entry {key} has no default translation ({DEFAULT_LOCALE}).")
                : value;
        }
    }

    public List<string> Languages { get; }
}

internal struct LocalesJson
{
    public List<string> Languages { get; set; }

    public Dictionary<string, LocalesEntry> Locales { get; set; }
}

internal sealed class LocalesEntry : Dictionary<string, string>;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LocalesJson))]
internal partial class LocalesJsonContext : JsonSerializerContext;