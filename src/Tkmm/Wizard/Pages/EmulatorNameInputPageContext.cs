using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.Wizard.Pages;

public sealed partial class EmulatorNameInputPageContext : ObservableObject
{
    private static readonly FilePickerFileType ExecutableFilePattern = new("Executable") {
        Patterns = [
            OperatingSystem.IsWindows() ? "*.exe" : "*"
        ]
    };

    [ObservableProperty]
    public partial string EmulatorName { get; set; } = string.Empty;

    public bool ShowLinuxAppImageWarning => OperatingSystem.IsLinux();

    [RelayCommand]
    private async Task Browse()
    {
        var result = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = Locale["SetupWizard_SelectEmulatorExecutable"],
            AllowMultiple = false,
            FileTypeFilter = [
                ExecutableFilePattern
            ]
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };

        if (result is not null) {
            EmulatorName = result;
        }
    }
}