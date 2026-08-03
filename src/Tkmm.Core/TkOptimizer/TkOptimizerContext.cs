using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Helpers;
using Tkmm.Core.TkOptimizer.Models;
using TkSharp.Core;
using TkSharp.Core.Models;
using TkSharp.IO.Writers;

namespace Tkmm.Core.TkOptimizer;

/// <summary>
/// TotK optimizer options template.
/// </summary>
public sealed class TkOptimizerContext : ObservableObject
{
    private static readonly SemaphoreSlim ApplyAsyncLock = new(1, 1);
    private readonly Dictionary<string, JsonElement> _optionValues = new(StringComparer.OrdinalIgnoreCase);
#if !SWITCH
    private string? _ephemeralSdCardRootPath;
#endif

    [NotNull]
    public TkOptimizerStore? Store {
        get => field ?? TkOptimizerStore.Current;
        set;
    }

    public ObservableCollection<TkOptimizerOptionGroup> Groups { get; } = [];

    public ObservableCollection<TkOptimizerCheatGroup> CheatGroups { get; } = [];

    internal List<TkOptimizerOption> AutoOptions { get; } = [];

    public bool IsEnabled {
        get => TkOptimizerStore.Current.IsEnabled;
        set {
            TkOptimizerStore.Current.IsEnabled = value;
            OnPropertyChanged();
        }
    }

    public string? Preset {
        get => TkOptimizerStore.Current.Preset;
        set {
            TkOptimizerStore.Current.Preset = value;
            OnPropertyChanged();
        }
    }

    public static TkOptimizerContext Create()
    {
        TkOptimizerContext context = new();
        TkOptimizerLoader.Load(context);
        return context;
    }

    internal bool TryGetOptionValue<T>(string key, out T value) where T : unmanaged
    {
        if (!_optionValues.TryGetValue(key, out var json)) {
            value = default;
            return false;
        }

        value = json.Deserialize<T>();
        return true;
    }

    internal void SetOptionValue<T>(string key, T value, bool writeOutput = true) where T : unmanaged
    {
        _optionValues[key] = JsonSerializer.SerializeToElement(value);
        if (writeOutput) {
            ApplyToSdCard();
        }
    }

    public void ApplyToSdCard()
    {
        _ = ApplyToSdCardAsync();
    }

    private async Task ApplyToSdCardAsync()
    {
        try {
            ITkModWriter writer = new FolderModWriter(TKMM.MergedOutputFolder);
            await ApplyAsync(writer, cancellationToken: CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Failed to export TotK Optimizer UltraCam configuration to SD paths.");
        }
    }

    public async ValueTask ApplyAsync(ITkModWriter mergeOutputWriter, TkProfile? profile = null,
        CancellationToken cancellationToken = default)
    {
        _ = mergeOutputWriter;

        await ApplyAsyncLock.WaitAsync(cancellationToken).ConfigureAwait(true);
        try {
            await ApplyCoreAsync(profile, cancellationToken).ConfigureAwait(true);
        }
        finally {
            ApplyAsyncLock.Release();
        }
    }

    private async ValueTask ApplyCoreAsync(TkProfile? profile, CancellationToken cancellationToken)
    {
        profile ??= TKMM.ModManager.GetCurrentProfile();

        if (!TkOptimizerStore.IsProfileEnabled(profile)) {
            return;
        }

        Store = TkOptimizerStore.CreateStore(profile);

#if !SWITCH
        if (!HasOutputDestination()) {
            if (TkOptimizerSdPrompt.RequestSdCardRootAsync is not { } requestRoot) {
                TkLog.Instance.LogWarning(
                    "TotK Optimizer configuration was not written: no SD card root or emulator SD path is configured, and no folder prompt is available.");
                Store = null;
                return;
            }

            var chosen = await requestRoot().ConfigureAwait(true);
            if (chosen is null || string.IsNullOrWhiteSpace(chosen.Path)) {
                TkLog.Instance.LogInformation("TotK Optimizer SD path selection was cancelled; configuration was not written.");
                Store = null;
                return;
            }

            if (chosen.PersistToConfig) {
                TkConfig.Shared.SdCardRootPath = chosen.Path;
                TkConfig.Shared.Save();
                _ephemeralSdCardRootPath = null;
            }
            else {
                _ephemeralSdCardRootPath = chosen.Path;
            }
        }
#endif

        var allOptions = Groups.SelectMany(x => x.Options).Concat(AutoOptions).ToArray();

        foreach (var optionsByFile in allOptions.GroupBy(x => x.OutputFileName, StringComparer.OrdinalIgnoreCase)) {
            cancellationToken.ThrowIfCancellationRequested();

            var fileOptions = optionsByFile.ToArray();
            var config = TkOptimizerConfigWriter.Build(fileOptions);
            TkOptimizerConfigWriter.ApplyModOverrides(config, optionsByFile.Key, fileOptions, profile);

            var outputSdFileName = Path.Combine("UltraCam", "TOTK", "Config", $"{optionsByFile.Key}.ini");

            using MemoryStream memoryStream = new();
            await using (StreamWriter writer = new(memoryStream, leaveOpen: true)) {
                TkOptimizerConfigWriter.Write(writer, config);
            }

#if !SWITCH
            if (!string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath))
            {
                var emulatorSdPath = TkEmulatorHelper.GetSdPath(Config.Shared.EmulatorPath);

                if (!string.IsNullOrWhiteSpace(emulatorSdPath)) {
                    var fullPath = Path.Combine(emulatorSdPath, outputSdFileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                    memoryStream.Position = 0;
                    await using var emulatorOutput = File.Create(fullPath);
                    await memoryStream.CopyToAsync(emulatorOutput, cancellationToken);
                }
            }
#endif

            var physicalSdRoot = GetSdRootForWrite();
            if (!string.IsNullOrWhiteSpace(physicalSdRoot)) {
                var fullPath = Path.Combine(physicalSdRoot, outputSdFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                memoryStream.Position = 0;
                await using var sdOutput = File.Create(fullPath);
                await memoryStream.CopyToAsync(sdOutput, cancellationToken);
            }
        }

        Store = null;
    }

#if !SWITCH
    private bool HasOutputDestination()
    {
        if (!string.IsNullOrWhiteSpace(TkConfig.Shared.SdCardRootPath)) {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(_ephemeralSdCardRootPath)) {
            return true;
        }

        if (string.IsNullOrWhiteSpace(Config.Shared.EmulatorPath)) {
            return false;
        }
        
        var emulatorSdPath = TkEmulatorHelper.GetSdPath(Config.Shared.EmulatorPath);
        return !string.IsNullOrWhiteSpace(emulatorSdPath);
    }
#endif

    private string? GetSdRootForWrite()
        => !string.IsNullOrWhiteSpace(TkConfig.Shared.SdCardRootPath)
            ? TkConfig.Shared.SdCardRootPath
#if !SWITCH
            : _ephemeralSdCardRootPath;
#else
            : null;
#endif

    public void Reload()
    {
        OnPropertyChanged(nameof(IsEnabled));
        OnPropertyChanged(nameof(Preset));
    }
}
