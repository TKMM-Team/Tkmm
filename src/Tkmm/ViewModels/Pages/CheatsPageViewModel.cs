using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using TkSharp.Core;

namespace Tkmm.ViewModels.Pages;

public partial class CheatsPageViewModel : ObservableObject
{
    public ObservableCollection<string> AvailableVersions { get; } = new ObservableCollection<string>();

    private string _selectedVersion;
    public string SelectedVersion
    {
        get => _selectedVersion;
        private set => SetProperty(ref _selectedVersion, value);
    }

    public ObservableCollection<CheatItemViewModel> Cheats { get; } = new ObservableCollection<CheatItemViewModel>();

    private readonly Dictionary<string, CheatVersion> _cheatVersions = new Dictionary<string, CheatVersion>();

    private readonly string _enabledCheatsFilePath = Path.Combine(AppContext.BaseDirectory, ".data", "enabled_cheats.json");

    private List<string> _persistedEnabledCheats = new List<string>();

    private bool _cheatsAvailable;
    public bool CheatsAvailable
    {
        get => _cheatsAvailable;
        private set => SetProperty(ref _cheatsAvailable, value);
    }

    public CheatsPageViewModel()
    {
        LoadCheatsCatalog();
    }

    private void LoadCheatsCatalog()
    {
        try
        {
            string resourcePath = "Tkmm.Resources.Optimizer.Cheats.json";
            using Stream? stream = typeof(TotKOptimizerPageViewModel).Assembly.GetManifestResourceStream(resourcePath);
            if (stream is null)
            {
                throw new FileNotFoundException("Cheats.json not found.");
            }
            using StreamReader reader = new(stream);
            string jsonText = reader.ReadToEnd();
            JsonDocument doc = JsonDocument.Parse(jsonText);

            foreach (JsonElement element in doc.RootElement.EnumerateArray())
            {
                if (!element.TryGetProperty("Aversion", out var aversionElem))
                    continue;
                string aversion = aversionElem.GetString();

                if (!element.TryGetProperty("Version", out var versionElem))
                    continue;
                string version = versionElem.GetString();

                if (!element.TryGetProperty("Source", out var sourceElem))
                    continue;
                string source = sourceElem.GetString();

                Dictionary<string, string> cheats = new Dictionary<string, string>();
                foreach (JsonProperty prop in element.EnumerateObject())
                {
                    if (prop.NameEquals("Aversion") || prop.NameEquals("Version") || prop.NameEquals("Source"))
                        continue;
                    string value = prop.Value.GetString();
                    cheats[prop.Name] = value;
                }

                var cheatVersion = new CheatVersion
                {
                    Aversion = aversion,
                    Version = version,
                    Source = source,
                    Cheats = cheats
                };

                _cheatVersions[aversion] = cheatVersion;
            }

            foreach (var key in _cheatVersions.Keys.OrderBy(x => x))
                AvailableVersions.Add(key);

            if (AvailableVersions.Any())
                SelectedVersion = AvailableVersions.First();

            _persistedEnabledCheats = LoadPersistedEnabledCheats();
            UpdateCheatsList();
        }
        catch (Exception ex)
        {
            TkLog.Instance.LogError(ex, "Error loading cheats catalog.");
        }
    }

    private List<string> LoadPersistedEnabledCheats()
    {
        try
        {
            if (File.Exists(_enabledCheatsFilePath))
            {
                string json = File.ReadAllText(_enabledCheatsFilePath);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
        }
        catch
        {
            // Ignoring errors and returning an empty list
        }
        return new List<string>();
    }

    private void UpdateCheatsList()
    {
        Cheats.Clear();
        if (!_cheatVersions.TryGetValue(SelectedVersion, out var cheatVersion))
            return;

        foreach (var pair in cheatVersion.Cheats.OrderBy(x => x.Key))
        {
            bool isEnabled = _persistedEnabledCheats.Contains(pair.Key);
            var cheatItem = new CheatItemViewModel
            {
                Name = pair.Key,
                Value = pair.Value,
                IsSelected = isEnabled
            };
            
            cheatItem.PropertyChanged += CheatItem_PropertyChanged;
            Cheats.Add(cheatItem);
        }
    }

    private void CheatItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CheatItemViewModel.IsSelected)) {
            _ = SaveCheatsAsync();
        }
    }

    private async Task SaveCheatsAsync()
    {
        if (!_cheatVersions.TryGetValue(SelectedVersion, out var cheatVersion))
            return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(cheatVersion.Source);
        sb.AppendLine();

        foreach (var cheat in Cheats.Where(c => c.IsSelected))
        {
            sb.AppendLine(cheat.Value.Trim());
            sb.AppendLine();
        }

        string outputDir = Path.Combine(TKMM.MergedOutputFolder, "cheats");
        Directory.CreateDirectory(outputDir);

        string outputFile = Path.Combine(outputDir, cheatVersion.Version + ".txt");

        await File.WriteAllTextAsync(outputFile, sb.ToString());

        var enabledCheats = Cheats.Where(c => c.IsSelected)
                                    .Select(c => c.Name)
                                    .ToList();
        string dataDir = Path.Combine(AppContext.BaseDirectory, ".data");
        Directory.CreateDirectory(dataDir);
        await File.WriteAllTextAsync(_enabledCheatsFilePath,
            JsonSerializer.Serialize(enabledCheats, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void RefreshVersion()
    {
        try
        {
            using ITkRom rom = TKMM.GetTkRom();
            int gameVersion = rom.GameVersion;

            int tens = (gameVersion / 10) % 10;
            int ones = gameVersion % 10;
            string cheatKey = $"1.{tens}.{ones}";

            if (_cheatVersions.TryGetValue(cheatKey, out var cheatVersion))
            {
                TkLog.Instance.LogInformation("Found matching cheat version: {CheatKey}", cheatKey);
                SelectedVersion = cheatKey;
                CheatsAvailable = true;
                _persistedEnabledCheats = LoadPersistedEnabledCheats();
                UpdateCheatsList();
            }
            else
            {
                TkLog.Instance.LogWarning("CheatsPageViewModel: No matching cheat version for key: {CheatKey}", cheatKey);
                CheatsAvailable = false;
            }
        }
        catch
        {
            CheatsAvailable = false;
        }
    }

    public partial class CheatItemViewModel : ObservableObject
    {
        public string Name { get; set; }
        public string Value { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    private class CheatVersion
    {
        public string Aversion { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }
        public Dictionary<string, string> Cheats { get; set; }
    }
}