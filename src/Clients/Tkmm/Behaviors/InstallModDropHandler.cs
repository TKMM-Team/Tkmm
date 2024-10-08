using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Tkmm.Abstractions;
using Tkmm.Components;
using Tkmm.Core;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Behaviors;

public class InstallModDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        return true;
    }

    public override async void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        // TODO: Handle errors here
        
        ITkMod? targetMod = null;

        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (string path in paths.Select(x => x.Path.LocalPath)) {
                if (await TKMM.ModManager.Install(path) is ITkMod mod) {
                    targetMod = mod;
                }
            }
        }
        else if (e.Data.GetText() is string arg) {
            if (await TKMM.ModManager.Install(arg) is ITkMod mod) {
                targetMod = mod;
            }
        }

        if (targetMod is null) {
            return;
        }

        PageManager.Shared.Get<HomePageViewModel>(Page.Home).Current = targetMod;
        var profilesPage = PageManager.Shared.Get<ProfilesPageViewModel>(Page.Profiles);
        profilesPage.MasterSelected = targetMod;
        profilesPage.Selected = targetMod;
    }
}
