using Avalonia;
using Avalonia.Data;
using AvaMark;
using Tkmm.Components;
using TkSharp.Core.Models;

namespace Tkmm.Controls;

public sealed class MarkdownView : AvaloniaObject
{
    static MarkdownView()
    {
        ModProperty.Changed.AddClassHandler<MarkdownViewer>(HandleModChanged);
    }
    
    public static readonly AttachedProperty<TkMod?> ModProperty = AvaloniaProperty.RegisterAttached<MarkdownView, MarkdownViewer, TkMod?>(
        "Mod", null, false, BindingMode.TwoWay);
    
    private static void HandleModChanged(MarkdownViewer viewer, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not TkMod mod) {
            return;
        }
        
        viewer.ImageResolver = TkImageResolver.Instance;
        viewer.ImageResolverState = mod.Id;
        viewer.Markdown = mod.Description;
    }
    
    public static void SetMod(AvaloniaObject element, TkMod mod)
        => element.SetValue(ModProperty, mod);

    public static TkMod? GetMod(AvaloniaObject element)
        => element.GetValue(ModProperty);
}