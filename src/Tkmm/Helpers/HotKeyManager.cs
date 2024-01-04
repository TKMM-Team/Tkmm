using Avalonia.Input;
using System.Windows.Input;

namespace Tkmm.Helpers;

// (c) NX-Editor, https://github.com/NX-Editor/NxEditor/blob/master/src/NxEditor/Components/HotKeyMgr.cs

public static class HotKeyManager
{
    public static Dictionary<KeyGesture, bool> HotKeys { get; } = [];
    public static Dictionary<string, List<KeyGesture>> HotKeyGroups { get; } = [];

    public static void RegisterHotKey(this InputElement target, KeyGesture hotKey, string group, ICommand command)
    {
        if (HotKeys.TryAdd(hotKey, true)) {
            if (!HotKeyGroups.TryGetValue(group, out List<KeyGesture>? hotKeys)) {
                hotKeys = [];
            }

            hotKeys.Add(hotKey);

            target.KeyDown += (s, e) => {
                if (!e.Handled && e.Key == hotKey.Key && e.KeyModifiers == hotKey.KeyModifiers && HotKeys[hotKey]) {
                    command.Execute(null);
                    e.Handled = true;
                }
            };
        }
    }

    public static void DisableHotKey(params KeyGesture[] keys)
    {
        foreach (var key in keys) {
            HotKeys[key] = false;
        }
    }

    public static void DisableHotKeyGroup(params string[] groups)
    {
        foreach (var group in groups) {
            if (!HotKeyGroups.TryGetValue(group, out List<KeyGesture>? keys)) {
                continue;
            }

            foreach (var key in keys) {
                HotKeys[key] = false;
            }
        }
    }
}