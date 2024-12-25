using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Microsoft.Extensions.Logging;
using Tkmm.Actions;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Behaviors;

public class InstallModDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        return true;
    }

    public override async void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        try {
            if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
                foreach (string path in paths.Select(item => item.Path.LocalPath)) {
                    await ModActions.Instance.Install(path);
                }
            }
            else if (e.Data.GetText() is string arg) {
                await ModActions.Instance.Install(arg);
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Failed to install drag/drop target: {TargetData}.", e.Data);
        }
    }
}
