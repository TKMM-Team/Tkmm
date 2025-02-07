using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Core.TkOptimizer.Models.ValueTypes;

public sealed partial class TkOptimizerBoolValue(bool @default) : TkOptimizerValue<bool>(@default);