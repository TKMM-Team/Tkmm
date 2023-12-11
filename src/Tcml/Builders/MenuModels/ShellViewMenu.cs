using System;
using Tcml.Attributes;

namespace Tcml.Builders.MenuModels;

public class ShellViewMenu
{
    [Menu("Exit", "File", "Alt + F4", "fa-solid fa-right-from-bracket")]
    public static void File_Exit()
    {
        // Handle exit cleanup here
        Environment.Exit(0);
    }
}
