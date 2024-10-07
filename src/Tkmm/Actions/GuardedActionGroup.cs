using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Tkmm.Core;

namespace Tkmm.Actions;

public abstract class GuardedActionGroup<TSingleton> where TSingleton : GuardedActionGroup<TSingleton>, new()
{
    protected abstract string ActionGroupName { get; }

    public static TSingleton Instance { get; } = new();

    /// <summary>
    /// Run pre-action checks to ensure the target action can run.
    /// </summary>
    protected async ValueTask<bool> CanActionRun([CallerMemberName] string? actionName = null, bool showError = true)
    {
        TKMM.Logger.LogInformation(
            "[TKMM] [{UtcNow}] Validatiing {ActionName} from {ActionGroupName}.",
            DateTime.UtcNow, actionName, ActionGroupName);
        
        LogInfo();

        if (EnsureConfiguration()) {
            TKMM.Logger.LogInformation(
                "[TKMM] [{UtcNow}] Executing {ActionName} from {ActionGroupName}.",
                DateTime.UtcNow, actionName, ActionGroupName);
            
            return true;
        }


        if (showError is false) {
            return false;
        }
        
        TKMM.Logger.LogCritical(
            "[TKMM] [{UtcNow}] Failed to run {ActionName} from {ActionGroupName}, the configuration was invalid.",
            DateTime.UtcNow, actionName, ActionGroupName);
        
        await ShowFailureDialog(actionName,
            "The application configuration is invalid, the invoked action cannot run.");
        
        return false;
    }

    /// <summary>
    /// Show a dialog with an error message.
    /// </summary>
    protected async ValueTask ShowFailureDialog(string? actionName, string errorMessage)
    {
        ContentDialog dialog = new() {
            Title = $"Failed to run {actionName}",
            Content = new TextBlock {
                Text = errorMessage,
                TextWrapping = TextWrapping.WrapWithOverflow
            },
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "OK"
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// Log information about the configured application for end-user debugging.
    /// </summary>
    public static void LogInfo()
    {
        TKMM.Romfs.LogInfo(TKMM.Logger);
        
        TKMM.Logger.LogInformation(
            "[TKMM] [{UtcNow}] Game Language: {GameLanguage}.", DateTime.UtcNow, TKMM.Config.GameLanguage);
    }

    /// <summary>
    /// Ensure the application is configured correctly.
    /// </summary>
    protected virtual bool EnsureConfiguration()
    {
        if (!TKMM.Romfs.IsStateValid(out string? invalidReason)) {
            TKMM.Logger.LogCritical("Invalid RomFS: {Reason}.", invalidReason);
            return false;
        }

        return true;
    }
}