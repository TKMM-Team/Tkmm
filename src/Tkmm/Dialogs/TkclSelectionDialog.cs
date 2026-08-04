using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using TkSharp.Core.Models;

namespace Tkmm.Dialogs;

public static class TkclSelectionDialog
{
    public static TkModContext AttachTo(TkModContext? context)
    {
        context ??= new TkModContext();
        context.SelectEmbeddedTkcl ??= SelectAsync;
        return context;
    }

    private static ValueTask<string?> SelectAsync(IReadOnlyList<string> candidates, CancellationToken ct = default)
    {
        return candidates.Count switch {
            0 => ValueTask.FromResult<string?>(null),
            1 => ValueTask.FromResult<string?>(candidates[0]),
            _ => new ValueTask<string?>(Dispatcher.UIThread.InvokeAsync(() => ShowAsync(candidates, ct)))
        };
    }

    private static async Task<string?> ShowAsync(IReadOnlyList<string> candidates, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var displayNames = GetDisplayNames(candidates);
        Dictionary<RadioButton, string> selectionMap = new(candidates.Count);
        StackPanel panel = new() {
            Spacing = 8,
            Orientation = Orientation.Vertical
        };

        for (var i = 0; i < candidates.Count; i++) {
            RadioButton radio = new() {
                Content = displayNames[i],
                GroupName = "TkclSelection",
                IsChecked = i == 0
            };
            selectionMap[radio] = candidates[i];
            panel.Children.Add(radio);
        }

        ContentDialog dialog = new() {
            Title = Locale["TkclSelection_Title"],
            Content = new ScrollViewer {
                Content = panel,
                MaxHeight = 360
            },
            PrimaryButtonText = Locale["TkclSelection_Install"],
            CloseButtonText = Locale["Action_Cancel"],
            DefaultButton = ContentDialogButton.Primary
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary) {
            return null;
        }

        foreach (var (radio, path) in selectionMap) {
            if (radio.IsChecked is true) {
                return path;
            }
        }

        return candidates[0];
    }

    private static string[] GetDisplayNames(IReadOnlyList<string> candidates)
    {
        var fileNames = candidates
            .Select(static path => Path.GetFileName(path.Replace('\\', '/')))
            .ToArray();

        var hasCollision = fileNames
            .GroupBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .Any(static group => group.Count() > 1);

        if (!hasCollision) {
            return fileNames;
        }

        return candidates
            .Select(static path => path.Replace('\\', '/'))
            .ToArray();
    }
}
