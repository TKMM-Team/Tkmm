#pragma warning disable SYSLIB1096 // TODO: Convert to 'GeneratedComInterface'

using System.Runtime.InteropServices;
using System.Text;

namespace Tkmm.Core.Helpers.Windows;

[ComImport]
[Guid("00021401-0000-0000-C000-000000000046")]
internal class ShellLink
{

}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214F9-0000-0000-C000-000000000046")]
internal interface IShellLink
{
    void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
    void GetIDList(out IntPtr ppidl);
    void SetIDList(IntPtr pidl);
    void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
    void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
    void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
    void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
    void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
    void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
    void GetHotkey(out short pwHotkey);
    void SetHotkey(short wHotkey);
    void GetShowCmd(out int piShowCmd);
    void SetShowCmd(int iShowCmd);
    void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
    void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
    void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
    void Resolve(IntPtr hwnd, int fFlags);
    void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("0000010b-0000-0000-C000-000000000046")]
public interface IPersistFile : IPersist
{
    // ReSharper disable once InconsistentNaming
    new void GetClassID(out Guid pClassID);
    [PreserveSig]
    int IsDirty();

    [PreserveSig]
    void Load([In, MarshalAs(UnmanagedType.LPWStr)]
        string pszFileName, uint dwMode);

    [PreserveSig]
    void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
        [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);

    [PreserveSig]
    void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

    [PreserveSig]
    void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("0000010c-0000-0000-c000-000000000046")]
public interface IPersist
{
    [PreserveSig]
    // ReSharper disable once InconsistentNaming
    void GetClassID(out Guid pClassID);
}