using Avalonia;
using Avalonia.Data;
using AvaMark;
using ReverseMarkdown;
using Tkmm.Components;
using Tkmm.Helpers;
using TkSharp.Core.Models;
using TkSharp.Extensions.GameBanana;

namespace Tkmm.Controls;

public abstract partial class MarkdownView : AvaloniaObject
{
    static MarkdownView()
    {
        ModProperty.Changed.AddClassHandler<MarkdownViewer>(HandleModChanged);
        GameBananaSubmissionProperty.Changed.AddClassHandler<MarkdownViewer>(HandleGameBananaSubmissionChanged);
    }
    
    public static readonly AttachedProperty<TkMod?> ModProperty = AvaloniaProperty.RegisterAttached<MarkdownView, MarkdownViewer, TkMod?>(
        "Mod", null, false, BindingMode.TwoWay);

    public static readonly AttachedProperty<GameBananaSubmission?> GameBananaSubmissionProperty =
        AvaloniaProperty.RegisterAttached<MarkdownView, MarkdownViewer, GameBananaSubmission?>(
            "GameBananaSubmission", null, false, BindingMode.TwoWay);

    private static void HandleModChanged(MarkdownViewer viewer, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not TkMod mod) {
            return;
        }
        
        viewer.ImageResolver = TkImageResolver.Instance;
        viewer.ImageResolverState = mod.Id;
        viewer.Markdown = ReplaceGameBananaUrls(mod.Description);
    }

    private static void HandleGameBananaSubmissionChanged(MarkdownViewer viewer, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not GameBananaSubmission submission) {
            return;
        }
        
        viewer.ImageResolver = TkImageResolver.Instance;
        viewer.ImageResolverState = submission.Id;

        var markdownContent = !string.IsNullOrEmpty(submission.Text)
            ? new Converter(new Config {
                GithubFlavored = true,
                ListBulletChar = '*',
                UnknownTags = Config.UnknownTagsOption.Bypass
            }).Convert(submission.Text)
            : submission.Description;
            
        viewer.Markdown = ReplaceGameBananaUrls(markdownContent);
    }
    
    
    private static string ReplaceGameBananaUrls(string markdownContent)
        => GameBananaUriHelper.ReplaceTkmmUrls(markdownContent);
    
    
    public static void SetMod(AvaloniaObject element, TkMod mod)
        => element.SetValue(ModProperty, mod);

    public static TkMod? GetMod(AvaloniaObject element)
        => element.GetValue(ModProperty);

    public static void SetGameBananaSubmission(AvaloniaObject element, GameBananaSubmission submission)
        => element.SetValue(GameBananaSubmissionProperty, submission);

    public static GameBananaSubmission? GetGameBananaSubmission(AvaloniaObject element)
        => element.GetValue(GameBananaSubmissionProperty);
}