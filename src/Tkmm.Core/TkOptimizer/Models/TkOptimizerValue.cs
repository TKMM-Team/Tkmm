using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.TkOptimizer.Models;

public abstract class TkOptimizerValue<T>(T @default) : TkOptimizerValue where T : unmanaged
{
    public T Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TkOptimizerStore.Current.TryGet(Key, out T value) ? value : @default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Set(value);
            OnPropertyChanged();
        }
    }
}

public abstract class TkOptimizerValue : ObservableObject
{
    internal string Key { get; set; } = null!;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Set<T>(T value) where T : unmanaged
    {
        TkOptimizerStore.Current.Set(Key, value);
    }
}