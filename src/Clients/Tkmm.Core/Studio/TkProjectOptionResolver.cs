using System.Text.Json;
using Tkmm.Core.Studio.Models;

namespace Tkmm.Core.Studio;

/// <summary>
/// Helper class to find and resolve option and option group in a project folder.
/// </summary>
public static class TkProjectOptionResolver
{
    public const string OPTION_FILE = ".tkoption";
    public const string OPTION_GROUP_FILE = ".tkgroup";
    
    public static async ValueTask ResolveProject(TkProject project, string projectFolder)
    {
        string optionsFolder = Path.Combine(projectFolder, "options");

        foreach (string optionGroupFolder in Directory.EnumerateDirectories(optionsFolder)) {
            project.Groups.Add(
                await OpenOptionGroup(optionGroupFolder)
            );
        }
    }
    
    public static async ValueTask<TkProjectOptionGroup> OpenOptionGroup(string optionGroupFolder)
    {
        string metadataPath = Path.Combine(optionGroupFolder, OPTION_GROUP_FILE);
        
        if (!File.Exists(metadataPath)) {
            return await CreateOptionGroup(optionGroupFolder);
        }

        await using FileStream fs = File.OpenRead(metadataPath);
        TkProjectOptionGroup? result = await JsonSerializer.DeserializeAsync(fs, TkProjectJsonContext.Default.TkProjectOptionGroup);

        if (result is null) {
            return await CreateOptionGroup(optionGroupFolder);
        }
        
        await ResolveOptionGroup(result, optionGroupFolder);
        return result;
    }
    
    public static async ValueTask<TkProjectOptionGroup> CreateOptionGroup(string optionGroupFolder)
    {
        TkProjectOptionGroup result = new() {
            Name = Path.GetFileNameWithoutExtension(optionGroupFolder)
        };

        await ResolveOptionGroup(result, optionGroupFolder);
        return result;
    }
    
    public static async ValueTask ResolveOptionGroup(TkProjectOptionGroup projectGroup, string optionGroupFolder)
    {
        foreach (string optionFolder in Directory.EnumerateDirectories(optionGroupFolder)) {
            projectGroup.Options.Add(
                await OpenOption(optionFolder)
            );
        }
    }
    
    public static async ValueTask<TkProjectOption> OpenOption(string optionFolder)
    {
        string metadataPath = Path.Combine(optionFolder, OPTION_FILE);
        
        if (!File.Exists(metadataPath)) {
            return CreateOption(optionFolder);
        }

        await using FileStream fs = File.OpenRead(metadataPath);
        return await JsonSerializer.DeserializeAsync(fs, TkProjectJsonContext.Default.TkProjectOption)
               ?? CreateOption(optionFolder);
    }
    
    public static TkProjectOption CreateOption(string optionFolder)
    {
        return new TkProjectOption {
            Name = Path.GetFileNameWithoutExtension(optionFolder)
        };
    }
}