using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;
using System.Collections;
using Tkmm.Core.Components;
using Tkmm.Core.Components.Models;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Behaviors;

public class ModsListBoxDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is ListBox listBox && listBox.ItemsSource is IList list && listBox.GetVisualAt(e.GetPosition(listBox)) is Control control) {
            Type? listBoxType = GetListBoxType(listBox);
            if (control.DataContext?.GetType() == listBoxType) {
                int currentIndex = list.IndexOf(sourceContext);
                int targetIndex = list.IndexOf(control.DataContext);

                if (currentIndex != targetIndex) {
                    (list[currentIndex], list[targetIndex]) = (list[targetIndex], list[currentIndex]);
                    listBox.SelectedIndex = targetIndex;
                }
            }

            return listBoxType == typeof(Mod) || listBoxType == typeof(ProfileMod);
        }

        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is ListBox listBox && listBox.ItemsSource is IList list) {
            Type? targetType = GetListBoxType(listBox);

            if (sourceContext is Mod mod && targetType == typeof(ProfileMod) && !list.Contains((ProfileMod)mod)) {
                list.Add((ProfileMod)mod);
                return true;
            }

            if (sourceContext is ProfileMod profileMod && targetType == typeof(Mod)) {
                ProfileManager.Shared.Current.Mods.Remove(profileMod);
                return true;
            }
        }

        return false;
    }

    private static Type? GetListBoxType(ListBox sender)
    {
        IEnumerable? src = sender.ItemsSource;

        if (src is null) {
            return null;
        }

        Type srcType = src.GetType();
        if (src.GetType().IsGenericType) {
            return srcType.GetGenericArguments().FirstOrDefault();
        }

        return null;
    }
}
