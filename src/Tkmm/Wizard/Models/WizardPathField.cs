using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Wizard.Models;

public sealed partial class WizardPathField : ObservableObject
{
    public string? Header { get; init; }

    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    public string? Watermark { get; init; }
    public required WizardBrowseOptions Browse { get; init; }
}