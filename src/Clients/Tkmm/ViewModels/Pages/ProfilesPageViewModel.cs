using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Abstractions;
using Tkmm.Actions;
using Tkmm.Core;
using Tkmm.Models.Mvvm;

namespace Tkmm.ViewModels.Pages;

public partial class ProfilesPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ITkMod> _filteredMods = GetOrderedMods();

    [ObservableProperty]
    private string? _filterArgument;

    public static IModManager ModManager => TKMM.ModManager;

    [RelayCommand]
    private static void Create()
    {
        TKMM.ModManager.Profiles.Add(
            new TkProfile()
        );
    }

    [RelayCommand]
    private static void AddToProfile(ITkMod mod)
    {
        ITkProfileMod target = mod.GetProfileMod();
        TKMM.ModManager.GetCurrentProfile().Mods.Add(target);
        TKMM.ModManager.GetCurrentProfile().Selected = target;
    }

    [RelayCommand]
    private static async Task Remove()
    {
        await ModActions.Instance.RemoveModFromProfile();
    }

    [RelayCommand]
    private static void MoveUp()
    {
        TKMM.ModManager.GetCurrentProfile().MoveUp();
    }

    [RelayCommand]
    private static void MoveDown()
    {
        TKMM.ModManager.GetCurrentProfile().MoveDown();
    }

    [RelayCommand]
    private static async Task Uninstall(ITkMod? target)
    {
        if (target is null) {
            return;
        }

        await ModActions.Instance.UninstallMod(target);
    }

    [RelayCommand]
    private static Task DeleteCurrentProfile()
    {
        return ProfileActions.Instance.DeleteProfile();
    }

    partial void OnFilterArgumentChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) {
            FilteredMods = GetOrderedMods();
            return;
        }

        FilteredMods = [..TKMM.ModManager.Mods
            .Where(mod => mod.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) || value.Contains(mod.Name, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(mod => mod.Name)
        ];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ObservableCollection<ITkMod> GetOrderedMods()
    {
        return [.. TKMM.ModManager.Mods.OrderBy(x => x.Name)];
    }
}
