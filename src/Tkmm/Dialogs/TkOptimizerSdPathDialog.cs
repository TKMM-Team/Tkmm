using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Tkmm.Core;
using Tkmm.Core.TkOptimizer;

namespace Tkmm.Dialogs;

public static class TkOptimizerSdPathDialog
{
    public static Task<TkOptimizerSdPathResult?> RequestSdCardRootAsync()
        => Dispatcher.UIThread.InvokeAsync(ShowAsync);

    private static async Task<TkOptimizerSdPathResult?> ShowAsync()
    {
        var pickedPath = TkConfig.Shared.SdCardRootPath;

        TextBox pathBox = new() {
            Text = pickedPath ?? string.Empty,
            IsReadOnly = true,
            MinHeight = 32,
            Watermark = Locale[TkLocale.TkConfig_SdCardRootPath]
        };

        Button browseButton = new() { Content = Locale[TkLocale.Tip_BrowseFolder] };
        browseButton.Click += async (_, _) => {
            var folders = await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions {
                    Title = Locale[TkLocale.TkConfig_SelectSdCardRoot],
                    AllowMultiple = false
                });

            pickedPath = folders switch {
                [var folder] => folder.TryGetLocalPath(),
                _ => pickedPath
            };

            pathBox.Text = pickedPath ?? string.Empty;
        };

        CheckBox saveForFutureCheckbox = new() {
            Content = Locale[TkLocale.TotkOptimizer_SaveSdPathForFuture],
            IsChecked = true
        };

        StackPanel layout = new() {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children = {
                new TextBlock {
                    Text = Locale[TkLocale.TotkOptimizer_NoSdCardRootMessage],
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 440
                },
                pathBox,
                browseButton,
                saveForFutureCheckbox
            }
        };

        ContentDialog dialog = new() {
            Title = Locale[TkLocale.TotkOptimizerPageTitle],
            Content = layout,
            PrimaryButtonText = "OK",
            CloseButtonText = Locale[TkLocale.Action_Cancel],
        };

        if (await dialog.ShowAsync() is not ContentDialogResult.Primary
            || string.IsNullOrWhiteSpace(pickedPath)) {
            return null;
        }

        var persistToConfig = saveForFutureCheckbox.IsChecked == true;

        return new TkOptimizerSdPathResult(pickedPath, persistToConfig);
    }
}
