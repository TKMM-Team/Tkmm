namespace Tkmm.Core.Abstractions;

public interface ITkProfileMod
{
    ITkMod Mod { get; set; }

    bool IsEnabled { get; set; }
}