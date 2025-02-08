namespace Tkmm.Core.TkOptimizer.Models.ValueTypes;

public sealed class TkOptimizerEnumValue(TkOptimizerContext context, int @default, List<TkOptimizerEnum> values) : TkOptimizerValue<int>(context, @default)
{
    public List<TkOptimizerEnum> Values { get; } = values;
}