using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.TkOptimizer.Models;

public abstract class TkOptimizerValue<T>(TkOptimizerContext context, T @default) : TkOptimizerValue(context) where T : unmanaged
{
    private readonly TkOptimizerContext _context = context;

    public T Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.Store.TryGet(Key, out T value) ? value : @default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            Set(value);
            OnPropertyChanged();
        }
    }
}

public abstract class TkOptimizerValue(TkOptimizerContext context) : ObservableObject
{
    internal string Key { get; set; } = null!;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Set<T>(T value) where T : unmanaged
    {
        context.Store.Set(Key, value);
    }
}