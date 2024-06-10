using System.Runtime.InteropServices;

namespace Tkmm.Core.Models;

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct TkclVersion
{
    [FieldOffset(0)]
    public int Value;

    [FieldOffset(0)]
    public short Major;

    [FieldOffset(2)]
    public byte Minor;

    [FieldOffset(3)]
    public byte Revision;

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Revision}";
    }
}
