using System.Runtime.InteropServices;

namespace Tkmm.Core.Helpers.Windows;

public enum WindowMode
{
    Hidden = 0,
    Visible = 5
}

public static partial class WindowsOperations
{
    private static readonly IntPtr _handle = GetConsoleWindow();
    private static WindowMode _current = WindowMode.Visible;

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetConsoleWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    // ReSharper disable once UnusedMethodReturnValue.Local
    private static partial bool ShowWindow(IntPtr windowHandle, int cmdShowMode);

    public static void SetWindowMode(WindowMode mode)
    {
        _current = mode;
        ShowWindow(_handle, (int)mode);
    }

    public static void SwapWindowMode()
    {
        _current = _current == WindowMode.Hidden
            ? WindowMode.Visible : WindowMode.Hidden;
        Config.Shared.ShowConsole = _current == WindowMode.Visible;
        ShowWindow(_handle, (int)_current);
    }
}