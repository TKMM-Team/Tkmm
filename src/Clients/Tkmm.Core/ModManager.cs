using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tkmm.Abstractions;
using Tkmm.Abstractions.IO;
using Tkmm.Abstractions.Providers;
using Tkmm.Core.IO.ModReaders;
using Tkmm.Core.IO.ModWriters;
using Tkmm.Models.Mvvm;

namespace Tkmm.Core;

public sealed class ModManager(IModReaderProvider readerProvider) : TkModStorage, IModManager
{
    private static readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".system", "profiles.json");
    
    public static readonly string SystemFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".system");
    public static readonly string SystemModsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".system", "mods");
    public static readonly string MergedModsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".merged");

    private readonly IModReaderProvider _readerProvider = readerProvider;

    public async Task Initialize(CancellationToken ct = default)
    {
        if (File.Exists(_path)) {
            await using FileStream fs = File.OpenRead(_path);
            TkModStorage? stored = await JsonSerializer.DeserializeAsync(
                fs, TkJsonContext.Default.TkModStorage, ct);

            if (stored is null || stored.Profiles.Count < 1) {
                goto Defaults;
            }

            foreach (ITkProfile profile in stored.Profiles) {
                Profiles.Add(profile);
            }

            CurrentProfile = Profiles.FirstOrDefault(profile => profile.Id == stored.CurrentProfile?.Id)
                             ?? Profiles[0];

            goto LoadMods;
        }

    Defaults:
        TkProfile defaultProfile = new() {
            Name = "Default"
        };

        Profiles.Add(defaultProfile);
        CurrentProfile = defaultProfile;

    LoadMods:
        if (!Directory.Exists(SystemModsFolder)) {
            return;
        }
        
        foreach (string modFolder in Directory.EnumerateDirectories(SystemModsFolder)) {
            if (await SystemModReader.Instance.ReadMod(modFolder, ct: ct) is not ITkMod mod) {
                TKMM.Logger.LogWarning(
                    "The mod folder {ModFolder} could not be read.", modFolder);
                continue;
            }
            
            Mods.Add(mod);
        }
    }

    public async Task Save(CancellationToken ct = default)
    {
        Directory.CreateDirectory(SystemFolder);
        
        await using FileStream fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, this, TkJsonContext.Default.TkModStorage, ct);
    }

    public async ValueTask Merge(ITkProfile profile, CancellationToken ct = default)
    {
        await TKMM.MergerMarshal.Merge(profile, MergedOutputModWriter.Instance, ct);
    }

    public async ValueTask<ITkMod?> Install<T>(T? input, Stream? stream = null, ModContext context = default, CancellationToken ct = default) where T : class
    {
        if (_readerProvider.GetReader(input) is not IModReader reader) {
            return null;
        }
        
        return await reader.ReadMod(input, stream, context, ct);
    }

    public ValueTask<(Stream Stream, int Size)> OpenModFile(ITkModChangelog target, string manifestFileName, CancellationToken ct = default)
    {
        string targetFile = Path.Combine(SystemModsFolder, target.Id.ToString(), manifestFileName);
        FileInfo targetFileInfo = new(targetFile);

        return ValueTask.FromResult<(Stream Stream, int Size)>(
            (Stream: targetFileInfo.OpenRead(), Size: Convert.ToInt32(targetFileInfo.Length))
        );
    }

    public IEnumerable<ITkModChangelog> GetConfiguredOptions(ITkMod target)
    {
        throw new NotImplementedException();
    }
}