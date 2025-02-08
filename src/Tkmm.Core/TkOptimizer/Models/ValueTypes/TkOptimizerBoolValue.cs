using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.TkOptimizer.Models.ValueTypes;

public sealed class TkOptimizerBoolValue(TkOptimizerContext context, bool @default) : TkOptimizerValue<bool>(context, @default);