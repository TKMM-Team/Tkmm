using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Tkmm.Core.Models.Mods;
using Tkmm.Core.Services;
using Tkmm.Helpers;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Behaviors;

public class InstallModDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        return e.Data.GetFiles()?.Any(x => File.Exists(x.Path.LocalPath) && ModReaderProviderService.GetReader(x.Path.LocalPath) is not null) == true
            || e.Data.GetText() is string text && ModReaderProviderService.GetReader(text) is not null;
    }

    public override async void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        Mod? targetMod = null;

        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (var path in paths.Select(x => x.Path.LocalPath)) {
                if (await ModHelper.Import(path) is Mod mod) {
                    targetMod = mod;
                }
            }
        }
        else if (e.Data.GetText() is string arg) {
            if (await ModHelper.Import(arg) is Mod mod) {
                targetMod = mod;
            }
        }

        if (targetMod is null) {
            return;
        }

        PageManager.Shared.Get<HomePageViewModel>(Page.Home).Current = targetMod;
        ProfilesPageViewModel profilesPage = PageManager.Shared.Get<ProfilesPageViewModel>(Page.Profiles);
        profilesPage.MasterSelected = targetMod;
        profilesPage.Selected = targetMod;
    }
}
