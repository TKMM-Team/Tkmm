using Avalonia;
using Avalonia.Data;
using AvaMark;
using ReverseMarkdown;
using System.Text.RegularExpressions;
using Tkmm.Components;
using TkSharp.Core.Models;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Controls;

public abstract partial class MarkdownView : AvaloniaObject
{
    static MarkdownView()
    {
        ModProperty.Changed.AddClassHandler<MarkdownViewer>(HandleModChanged);
        GameBananaModProperty.Changed.AddClassHandler<MarkdownViewer>(HandleGameBananaModChanged);
    }
    
    public static readonly AttachedProperty<TkMod?> ModProperty = AvaloniaProperty.RegisterAttached<MarkdownView, MarkdownViewer, TkMod?>(
        "Mod", null, false, BindingMode.TwoWay);
    
    public static readonly AttachedProperty<GameBananaMod?> GameBananaModProperty = AvaloniaProperty.RegisterAttached<MarkdownView, MarkdownViewer, GameBananaMod?>(
        "GameBananaMod", null, false, BindingMode.TwoWay);
    
    private static void HandleModChanged(MarkdownViewer viewer, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not TkMod mod) {
            return;
        }
        
        viewer.ImageResolver = TkImageResolver.Instance;
        viewer.ImageResolverState = mod.Id;
        viewer.Markdown = ReplaceGameBananaUrls(mod.Description);
    }
    
    private static void HandleGameBananaModChanged(MarkdownViewer viewer, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not GameBananaMod mod) {
            return;
        }
        
        viewer.ImageResolver = TkImageResolver.Instance;
        viewer.ImageResolverState = mod.Id;
        
        var markdownContent = !string.IsNullOrEmpty(mod.Text) 
            ? new Converter(new Config {
                GithubFlavored = true,
                ListBulletChar = '*',
                UnknownTags = Config.UnknownTagsOption.Bypass
            }).Convert(mod.Text)
            : mod.Description;
            
        viewer.Markdown = ReplaceGameBananaUrls(markdownContent);
        
    }
    
    
    private static string ReplaceGameBananaUrls(string markdownContent)
    {
        return GbUrlRegex().Replace(markdownContent, match => $"tkmm://mod/{match.Groups[1].Value}");
    }
    
    
    public static void SetMod(AvaloniaObject element, TkMod mod)
        => element.SetValue(ModProperty, mod);

    public static TkMod? GetMod(AvaloniaObject element)
        => element.GetValue(ModProperty);
        
    public static void SetGameBananaMod(AvaloniaObject element, GameBananaMod mod)
        => element.SetValue(GameBananaModProperty, mod);

    public static GameBananaMod? GetGameBananaMod(AvaloniaObject element)
        => element.GetValue(GameBananaModProperty);
    
    [GeneratedRegex(@"https?://gamebanana\.com/mods/(\d+)")]
    private static partial Regex GbUrlRegex();
}