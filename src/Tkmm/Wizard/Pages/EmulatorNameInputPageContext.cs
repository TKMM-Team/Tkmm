using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Wizard.Pages;

public sealed partial class EmulatorNameInputPageContext : ObservableObject
{
    [ObservableProperty]
    private string _emulatorName = string.Empty;
} 