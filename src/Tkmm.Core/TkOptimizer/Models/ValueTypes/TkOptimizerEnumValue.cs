namespace Tkmm.Core.TkOptimizer.Models.ValueTypes;

public sealed class TkOptimizerEnumValue(int @default, List<TkOptimizerEnum> values) : TkOptimizerValue<int>(@default)
{
    public List<TkOptimizerEnum> Values { get; } = values;
}