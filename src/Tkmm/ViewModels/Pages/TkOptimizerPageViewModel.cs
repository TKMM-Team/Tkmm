using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.Services;
using Tkmm.Core.TkOptimizer;

namespace Tkmm.ViewModels.Pages;

public class TkOptimizerPageViewModel : ObservableObject
{
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public TkOptimizerContext Context => TkOptimizerService.Context;

    public void Reload()
    {
        Context.Reload();
    }
}