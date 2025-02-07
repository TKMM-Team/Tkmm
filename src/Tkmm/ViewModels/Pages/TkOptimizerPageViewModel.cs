using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core.TkOptimizer;

namespace Tkmm.ViewModels.Pages;

public class TkOptimizerPageViewModel : ObservableObject
{
    public TkOptimizerContext Context { get; } = TkOptimizerContext.Create();

    public void Reload()
    {
        TkOptimizerStore.ResetCurrent();
        Context.Reload();
    }
}