#if !SWITCH
namespace Tkmm.Wizard;

public enum DumpSource {
    Ryujinx = 1,
    Switch = 2,
    Other = 3
}

public enum BaseGameDumpType {
    XciNsp,
    Romfs,
    SdCard,
    Nand
}

public enum UpdateDumpType {
    Nsp,
    SdCard,
    Nand
}
#endif
