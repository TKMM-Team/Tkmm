using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tkmm.Core;
using Tkmm.Core.Abstractions;
using Tkmm.Dialogs;

namespace Tkmm.Actions;

public sealed partial class MergeActions : ActionsBase<MergeActions>
{
    protected override string ActionGroupName { get; } = nameof(MergeActions).Humanize();
    
    [RelayCommand]
    public async Task Merge(CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        try {
            await TKMM.ModManager.Merge(ct);
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured when merging the selected profile '{Profile}'.", TKMM.ModManager.CurrentProfile.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    } 
    
    [RelayCommand]
    public async Task MergeProfile(ITkProfile profile, CancellationToken ct = default)
    {
        if (!await CanActionRun()) {
            return;
        }

        try {
            await TKMM.ModManager.Merge(profile, ct);
        }
        catch (Exception ex) {
            TKMM.Logger.LogError(ex, "An error occured when merging the profile '{Profile}'.", profile.Name);
            await ErrorDialog.ShowAsync(ex);
        }
    }
}