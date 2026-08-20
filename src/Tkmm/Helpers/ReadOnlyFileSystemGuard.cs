using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
#if SWITCH
using Avalonia.Layout;
using Tkmm.Controls;
using Tkmm.Models.MenuModels;
#endif

namespace Tkmm.Helpers;

public static class ReadOnlyFileSystemGuard
{
#if SWITCH
    private const string TROUBLESHOOTING_URL =
        "https://tkmm.org/tkmm-nx/troubleshooting/#method-1-scanning-and-repairing-the-sd-card";
#endif

    public static bool IsPending { get; private set; }

    public static void Detect(string baseDirectory)
    {
        IsPending = !DataFolderGuard.IsDataFolderWritable(baseDirectory);
    }

    public static void Apply(Window shellView)
    {
        if (shellView.Content is Control content) {
            content.IsEnabled = false;
        }

        shellView.Opened += OnShellOpened;
    }

    private static void OnShellOpened(object? sender, EventArgs e)
    {
        if (sender is Window window) {
            window.Opened -= OnShellOpened;
        }

        Dispatcher.UIThread.InvokeAsync(ShowBlockingErrorAndExit);
    }

    private static async Task ShowBlockingErrorAndExit()
    {
        ContentDialog dialog = new() {
            Title = Locale["ReadOnlyFileSystemGuard_Title"],
            Content = BuildDialogContent(),
#if SWITCH
            PrimaryButtonText = Locale["Menu_NxReboot"],
#else
            PrimaryButtonText = Locale["Action_Close"],
#endif
            DefaultButton = ContentDialogButton.Primary
        };

        try {
            await dialog.ShowAsync();
        }
        finally {
#if SWITCH
            NxMenuModel.Reboot();
#else
            Exit();
#endif
        }
    }

    private static Control BuildDialogContent()
    {
#if SWITCH
        return new StackPanel {
            Spacing = 16,
            Children = {
                new TextBlock {
                    Text = Locale["ReadOnlyFileSystemGuard_MessageNx"],
                    TextWrapping = TextWrapping.WrapWithOverflow
                },
                new BrandedQrCode {
                    Width = 200,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Url = TROUBLESHOOTING_URL
                }
            }
        };
#else
        return new TextBlock {
            Text = Locale["ReadOnlyFileSystemGuard_Message"],
            TextWrapping = TextWrapping.WrapWithOverflow
        };
#endif
    }

    private static void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Shutdown(1);
            return;
        }

        Environment.Exit(1);
    }
}