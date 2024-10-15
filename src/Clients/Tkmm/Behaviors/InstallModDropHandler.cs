using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Tkmm.Abstractions;
using Tkmm.Actions;

namespace Tkmm.Behaviors;

public class InstallModDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        return true;
    }

    public override async void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        ITkMod? targetMod = null;

        if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
            foreach (string path in paths.Select(item => item.Path.LocalPath)) {
                if (await ModActions.Instance.Install(path) is ITkMod mod) {
                    targetMod = mod;
                }
            }
        }
        else if (e.Data.GetText() is string arg) {
            if (await ModActions.Instance.Install(arg) is ITkMod mod) {
                targetMod = mod;
            }
        }

        if (targetMod is null) {
            return;
        }
    }
}
