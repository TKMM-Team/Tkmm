global using static Tkmm.Core.Localization.StringResources;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LocalizationResources = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;

namespace Tkmm.Core.Localization;

public static class StringResources
{
    private static readonly string _localizationsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Languages");
    private static readonly LocalizationResources _resources = LoadResources();

    public static readonly StringResources_Exceptions Exceptions = new();
    public static readonly StringResources_Pages PageMsg = new();
    public static readonly StringResources_Status StatusMsg = new();
    public static readonly StringResources_System SystemMsg = new();

    internal static LocalizationResources LoadResources()
    {
#if RELEASE
        string configuredLocalizationFilePath = Path.Combine(_localizationsFolderPath, $"{TKMM.Config.CultureName}.json");
        if (File.Exists(configuredLocalizationFilePath)) {
            using FileStream configuredLocalizationFileStream = File.OpenRead(configuredLocalizationFilePath);
            return JsonSerializer.Deserialize<LocalizationResources>(configuredLocalizationFileStream) ?? [];
        }
#endif

        string defaultLocalizationFilePath = Path.Combine(_localizationsFolderPath, "en-US.json");
        using Stream defaultLocalizationFileStream = File.OpenRead(defaultLocalizationFilePath);
        return JsonSerializer.Deserialize<LocalizationResources>(defaultLocalizationFileStream) ?? [];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetStringResource(string group, string name)
    {
        return _resources[group][name];
    }
}