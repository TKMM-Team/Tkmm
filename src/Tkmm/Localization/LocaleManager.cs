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
        using Stream embeddedStream = typeof(LocaleManager).Assembly
            .GetManifestResourceStream("Tkmm.Resources.locales.json") ?? throw new NullReferenceException("Locale is not embedded.");

        LocalesJson json = JsonSerializer.Deserialize(embeddedStream, LocalesJsonContext.Default.LocalesJson);
        return new LocaleManager(json);
    }

    private LocaleManager(LocalesJson json)
    {
        Languages = json.Languages;
        _entries = json.Locales;
        // get the system language from CultureInfo.CurrentUICulture
        if (!Config.Shared.ConfigExists()) {
            string systemLang = GetSystemDefaultLanguage();
            _currentCulture = systemLang.Replace('-', '_');
        } else {
            _currentCulture = Config.Shared.CultureName.Value.Replace('-', '_');
        }

        Config.Shared.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(Config.Shared.CultureName)) {
                _currentCulture = Config.Shared.CultureName.Value.Replace('-', '_');
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("Translation");
            }
        };
    }

    private static string GetSystemDefaultLanguage()
    {
        try {
            // Get the current culture and map it to a supported language
            string currentCulture = System.Globalization.CultureInfo.CurrentUICulture.Name;
            string langCode = currentCulture.Split('-')[0].ToLowerInvariant();
            string countryCode = currentCulture.Contains("-") ? currentCulture.Split('-')[1].ToUpperInvariant() : "";
            
            return langCode switch
            {
                "en" => countryCode == "GB" ? "en_GB" : "en_US",
                "ja" => "ja_JP",
                "fr" => countryCode == "CA" ? "fr_CA" : "fr_FR",
                "es" => countryCode == "MX" ? "es_MX" : "es_ES",
                "de" => "de_DE",
                "nl" => "nl_NL",
                "it" => "it_IT",
                "ru" => "ru_RU",
                "ko" => "ko_KR",
                "zh" => countryCode == "TW" ? "zh_TW" : "zh_CN",
                _ => "en_US"  // Default to English if no matching language
            };
        }
        catch {
            // If anything goes wrong, default to English
            return "en_US";
        }
    }

    public string this[TkLocale key] => this[Enum.GetName(key)!];

    public string this[TkLocale key, params object[] arguments] => string.Format(this[key], arguments);

    public string this[string key] => this[key, failSoftly: false, culture: null];

    public string this[string key, bool failSoftly] => this[key, failSoftly, culture: null];

    public string this[string key, bool failSoftly, string? culture] {
        get {
            if (!_entries.TryGetValue(key, out LocalesEntry? entry)) {
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