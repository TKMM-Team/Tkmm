using CommunityToolkit.Mvvm.ComponentModel;
using Humanizer;
using TkSharp.Core;

namespace Tkmm.Core.TkOptimizer.Models;

public sealed class TkOptimizerCheat(TkOptimizerContext context, TkOptimizerCheatGroup group, string key, TkCheat tkCheat) : ObservableObject
{
    public string Key { get; } = key;

    public string Name => GetLocaleOrDefault($"TkOptimizer_Cheats_{Key.Dehumanize()}_Name", Key);

    public TkCheat TkCheat { get; set; } = tkCheat;

    public bool IsEnabled {
        get => context.Store.GetCheat(group, Key);
        set {
            context.Store.SetCheat(group, Key, value);
            OnPropertyChanged();
        }
    }
}