using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Dialogs;
using TotkCommon;
using TotkCommon.Components;

namespace Tkmm.Actions;

public sealed partial class ValidationActions : GuardedActionGroup<ValidationActions>
{
    protected override string ActionGroupName { get; } = nameof(ValidationActions).Humanize();

    [RelayCommand]
    public async Task CheckAndReportDumpIntegrity()
    {
        await CanActionRun(showError: false);
        
        TkStatus.Set("Checking dump integrity",
            "fa-solid fa-file-magnifying-glass", StatusType.Working);

        TotkDumpResults results;
        try {
            results = await Task.Run(async () => TotkDump.CheckIntegrity(
                Totk.Config.GamePath,
                await DumpIntegrityHelper.GetChecksumTable(),
                CheckDumpIntegrityUpdateCallback
            ));
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "Dump integrity check failed.");
            await ErrorDialog.ShowAsync(ex);
            return;
        }

        TkStatus.SetTemporary("Dump integrity check completed",
            TkIcons.CIRCLE_CHECK, duration: 3.5);
        
        string formattedResults = DumpIntegrityHelper.FormatResults(results);
        ContentDialog dialog = new() {
            Title = "Dump Integrity Check Results",
            Content = new StackPanel {
                Children = {
                    new TextBlock {
                        Margin = new Thickness(0, 0, 0, 5),
                        Text = results.IsCompleteDump switch {
                            true => "Complete game dump",
                            false => "Incomplete or corrupt game dump" 
                        },
                        FontSize = 16
                    },
                    new Button {
                        Margin = new Thickness(0, 0, 0, 5),
                        Padding = new Thickness(5, 2),
                        Content = "Copy Report",
                        Command = new AsyncRelayCommand(async () => { 
                            if (App.XamlRoot.Clipboard?.SetTextAsync(formattedResults) is Task task) {
                                await task;
                            }
                        })
                    },
                    new TextBlock {
                        TextWrapping = TextWrapping.WrapWithOverflow,
                        Text = formattedResults
                    },
                }
            },
            IsSecondaryButtonEnabled = false,
            PrimaryButtonText = "OK",
            IsPrimaryButtonEnabled = true,
            DefaultButton = ContentDialogButton.Primary
        };

        await dialog.ShowAsync();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckDumpIntegrityUpdateCallback(int i, int length)
    {
        TkStatus.Set($"Checking {i}/{length}...", TkIcons.PROGRESS);
    }
}