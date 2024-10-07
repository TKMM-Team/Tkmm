using System.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Tkmm.Core;
using Tkmm.Core.Abstractions;

namespace Tkmm.Behaviors;

public class ModsListBoxDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is not ListBox { ItemsSource: IList list } listBox || listBox.GetVisualAt(e.GetPosition(listBox)) is not Control control) {
            return false;
        }

        Type? listBoxType = GetListBoxType(listBox);
        if (control.DataContext?.GetType() != listBoxType) {
            goto DefaultCheck;
        }

        int currentIndex = list.IndexOf(sourceContext);
        int targetIndex = list.IndexOf(control.DataContext);

        if (currentIndex == -1 || targetIndex == -1 || currentIndex == targetIndex) {
            goto DefaultCheck;
        }

        (list[currentIndex], list[targetIndex]) = (list[targetIndex], list[currentIndex]);
        listBox.SelectedIndex = targetIndex;

    DefaultCheck:
        return listBoxType == typeof(ITkMod) || listBoxType == typeof(ITkProfileMod);

    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is not ListBox { ItemsSource: IList list } listBox) {
            return false;
        }

        Type? targetType = GetListBoxType(listBox);

        if (sourceContext is ITkMod mod && targetType == typeof(ITkProfileMod) && mod.GetProfileMod() is ITkProfileMod newProfileMod && !list.Contains(newProfileMod)) {
            list.Add(newProfileMod);
            return true;
        }

        if (sourceContext is not ITkProfileMod profileMod || targetType != typeof(ITkMod)) {
            return false;
        }

        TKMM.ModManager.CurrentProfile.Mods.Remove(profileMod);
        return true;

    }

    private static Type? GetListBoxType(ListBox sender)
    {
        IEnumerable? src = sender.ItemsSource;

        if (src is null) {
            return null;
        }

        Type srcType = src.GetType();
        return src.GetType().IsGenericType ? srcType.GetGenericArguments().FirstOrDefault() : null;
    }
}
