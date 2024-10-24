using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tkmm.Core.Studio.Models;

namespace Tkmm.Core.Studio;

public sealed class TkProjectManager
{
    private static readonly string _path = Path.Combine(AppContext.BaseDirectory, "projects.json");
    
    public ObservableCollection<string> RecentProjects { get; set; } = [];

    public static async ValueTask<TkProject?> OpenProject(string tkProjectFilePath)
    {
        if (Path.GetDirectoryName(tkProjectFilePath) is not string projectFolder) {
            return null;
        }
        
        await using FileStream fs = File.OpenRead(tkProjectFilePath);
        TkProject? project = await JsonSerializer.DeserializeAsync(fs, TkProjectJsonContext.Default.TkProject);

        if (project is null) {
            return await CreateProject(projectFolder);
        }
        
        await TkProjectOptionResolver.ResolveProject(project, projectFolder);
        return project;
    }

    public static async ValueTask<TkProject> CreateProject(string projectFolder)
    {
        string name = Path.GetFileNameWithoutExtension(projectFolder);
        TkProject result = new() {
            Name = name
        };

        await TkProjectOptionResolver.ResolveProject(result, projectFolder);
        return result;
    }

    public async ValueTask Save()
    {
        await using FileStream fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, this, TkProjectJsonContext.Default.TkProjectManager);
    }
}

[JsonSerializable(typeof(TkProject))]
[JsonSerializable(typeof(TkProjectOption))]
[JsonSerializable(typeof(TkProjectOptionGroup))]
[JsonSerializable(typeof(TkProjectManager))]
public sealed partial class TkProjectJsonContext : JsonSerializerContext;