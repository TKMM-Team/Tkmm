using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.TkOptimizer.Models.ValueTypes;

public sealed partial class TkOptimizerRangeValue(TkOptimizerContext context, int @default) : TkOptimizerValue<int>(context, @default)
{
    [ObservableProperty]
    private int _minValue;
    
    [ObservableProperty]
    private int _maxValue;
    
    [ObservableProperty]
    private int _incrementSize;
}