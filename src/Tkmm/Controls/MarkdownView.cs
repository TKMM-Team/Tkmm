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
        viewer.Markdown = mod.Description;
    }
    
    private static void HandleGameBananaModChanged(MarkdownViewer viewer, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not GameBananaMod mod) {
            return;
        }
        
        viewer.ImageResolver = TkImageResolver.Instance;
        viewer.ImageResolverState = mod.Id;
        
        string markdownContent = !string.IsNullOrEmpty(mod.Text) 
            ? new Converter(new Config {
                GithubFlavored = true,
                ListBulletChar = '*',
                UnknownTags = Config.UnknownTagsOption.Bypass
            }).Convert(mod.Text)
            : mod.Description;
            
        markdownContent = ReplaceGameBananaUrls(markdownContent);
            
        viewer.Markdown = markdownContent;
        
    }
    
    
    private static string ReplaceGameBananaUrls(string markdownContent)
    {
        return GbUrlRegex().Replace(markdownContent, match =>
        {
            var linkText = match.Groups[1].Value;
            var modId = match.Groups[3].Value;
            
            return IsModForTotk(modId) ? $"[{linkText}](tkmm://mod/{modId})" : match.Value;
        });
    }
    
    private static bool IsModForTotk(string modId)
    {
        try {
            var apiUrl = $"https://gamebanana.com/apiv11/Mod/{modId}/ProfilePage";
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var response = client.GetStringAsync(apiUrl).GetAwaiter().GetResult();
            var json = System.Text.Json.JsonDocument.Parse(response);
            
            if (json.RootElement.TryGetProperty("_aGame", out var gameElement) && 
            gameElement.TryGetProperty("_idRow", out var gameIdElement) &&
            gameIdElement.GetInt32() == 7617) {
                return true;
            }
        }
        catch {
            return false;
        }
        
        return false;
    }
    
    
    public static void SetMod(AvaloniaObject element, TkMod mod)
        => element.SetValue(ModProperty, mod);

    public static TkMod? GetMod(AvaloniaObject element)
        => element.GetValue(ModProperty);
        
    public static void SetGameBananaMod(AvaloniaObject element, GameBananaMod mod)
        => element.SetValue(GameBananaModProperty, mod);

    public static GameBananaMod? GetGameBananaMod(AvaloniaObject element)
        => element.GetValue(GameBananaModProperty);
    
    [GeneratedRegex(@"\[([^\]]+)\]\((https?://gamebanana\.com/mod(?:s)?/(\d+))\)")]
    private static partial Regex GbUrlRegex();
}