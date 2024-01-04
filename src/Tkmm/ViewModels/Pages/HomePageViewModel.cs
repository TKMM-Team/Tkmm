using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tkmm.Core;
using Tkmm.Core.Helpers.Operations;
using Tkmm.Models.Mods;

namespace Tkmm.ViewModels.Pages;

public partial class HomePageViewModel : ObservableObject
{
    [ObservableProperty]
    private Mod? _currentMod;

    [ObservableProperty]
    private ObservableCollection<Mod> _mods = [];

    [RelayCommand]
    private async Task ShowContributors()
    {
        ContentDialog dialog = new() {
            Title = "Contributors",
            Content = new TextBlock {
                Text = $"""
                {string.Join("\n", CurrentMod?.Contributors
                    .Select(x => $"{x.Name}: {string.Join(", ", x.Contributions)}") ?? [])}
                """,
                TextWrapping = TextWrapping.WrapWithOverflow
            },
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "Dismiss"
        };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private Task Apply()
    {
        // This should be abstracted to a
        // service but this should work

        string modList = Path.Combine(Config.Shared.StorageFolder, "mods.json");
        using FileStream fs = File.Create(modList);

        JsonSerializer.Serialize(fs, Mods.Select(x => DirectoryOperations
            .ToSafeName(x.Name)
            .Replace(' ', '-')
            .ToLower())
        );

        AppStatus.Set("Saved mods profile!", "fa-solid fa-list-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MoveUp()
    {
        Move(-1);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MoveDown()
    {
        Move(1);
        return Task.CompletedTask;
    }

    private void Move(int offset)
    {
        if (CurrentMod is null) {
            return;
        }

        int currentIndex = Mods.IndexOf(CurrentMod);
        int newIndex = currentIndex + offset;

        if (newIndex < 0 || newIndex >= Mods.Count) {
            return;
        }

        Mod store = Mods[newIndex];
        Mods[newIndex] = CurrentMod;
        Mods[currentIndex] = store;

        CurrentMod = Mods[newIndex];
    }

    public HomePageViewModel()
    {
        // Load the mod list when the home
        // page view model is first initialized

        // This can be replaced with an
        // active profile later
        string modList = Path.Combine(Config.Shared.StorageFolder, "mods.json");
        if (!File.Exists(modList)) {
            return;
        }

        using FileStream fs = File.OpenRead(modList);
        List<string> mods = JsonSerializer.Deserialize<List<string>>(fs)
            ?? [];

        foreach (string mod in mods) {
            string modFolder = Path.Combine(Config.Shared.StorageFolder, "mods", mod);
            if (Directory.Exists(modFolder)) {
                Mods.Add(Mod.FromFolder(modFolder));
            }
        }

        CurrentMod = Mods.FirstOrDefault();
    }
}
