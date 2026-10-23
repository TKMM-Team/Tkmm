using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Wizard.Models;

public sealed partial class WizardRadioOption : ObservableObject
{
    public required string Content { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public bool IsEnabled { get; init; } = true;
    public bool IsVisible { get; init; } = true;
    public object? Tag { get; init; }

    public static WizardRadioOption Opt(TkLocale content, object tag, bool selected = false)
        => Opt(Locale[content], tag, selected);

    public static WizardRadioOption Opt(string content, object tag, bool selected = false)
        => new() { Content = content, Tag = tag, IsSelected = selected };
}