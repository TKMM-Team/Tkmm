using Avalonia.Platform.Storage;
using LibHac.Common.Keys;
using Tkmm.Helpers;

namespace Tkmm.Wizard.Actions;

public static class WizActions
{
    private static readonly FilePickerFileType _exe = new("Executable") {
        Patterns = [
            OperatingSystem.IsWindows() ? "*.exe" : "*"
        ]
    };
    
    public static async ValueTask<(bool, int?)> SetupOtherEmulator()
    {
        string? emulatorFilePath = await App.XamlRoot.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = "Select emulator executable",
            AllowMultiple = false,
            FileTypeFilter = [
                _exe
            ]
        }) switch {
            [IStorageFile target] => target.TryGetLocalPath(),
            _ => null
        };

        if (emulatorFilePath is null) {
            // TODO: (Dialog)
            return (false, null);
        }
        
        // TODO: Persist exe location
        return (true, null);
    }
    
    public static async ValueTask<(bool, int?)> StartRyujinxSetup()
    {
        if (RyujinxHelper.GetRyujinxDataFolder() is not string ryujinxDataFolder) {
            // TODO: (Dialog)
            return (false, null);
        }

        if (RyujinxHelper.GetKeys(ryujinxDataFolder, out string ryujinxKeysFolder) is not KeySet keys) {
            // TODO: (Dialog)
            return (false, null);
        }
        
        // TODO: Persist Ryujinx config
        return (true, null);
    }
    
    public static async ValueTask<(bool, int?)> VerifyConfig()
    {
        throw new NotImplementedException();
    }
}