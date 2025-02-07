using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.TkOptimizer.Models.ValueTypes;

public sealed partial class TkOptimizerFloatingPointRangeValue(double @default) : TkOptimizerValue<double>(@default)
{
    [ObservableProperty]
    private double _minValue;
    
    [ObservableProperty]
    private double _maxValue;
}