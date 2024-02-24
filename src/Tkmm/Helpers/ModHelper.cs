using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Generics;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Helpers;

public static class ModHelper
{
    private static readonly Bitmap _defaultThumbnail;

    static ModHelper()
    {
        Mod.ResolveThumbnail = ResolveThumbnail;

        using Stream stream = AssetLoader.Open(new("avares://Tkmm/Assets/DefaultThumbnail.jpg"));
        _defaultThumbnail = new Bitmap(stream);
    }

    public static async Task<Mod?> Import(string arg)
    {
        try {
            AppStatus.Set($"Installing '{arg}'", "fa-solid fa-download", isWorkingStatus: true);

            Mod result = await Task.Run(async
                () => await ImportAsync(arg)
            );

            AppStatus.Set("Install Complete!", "fa-regular fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
            return result;
        }
        catch (Exception ex) {
            App.ToastError(ex);
            AppStatus.Set("Install Failed!", "fa-regular fa-circle-xmark", isWorkingStatus: false, temporaryStatusTime: 1.5);
        }

        return null;
    }

    public static async Task ResolveThumbnail(Mod mod)
    {
        if (mod.Thumbnail is not null) {
            return;
        }

        if (mod.ThumbnailUri is string uri) {
            string localPath = Path.Combine(mod.SourceFolder, uri);
            if (File.Exists(localPath)) {
                mod.Thumbnail = new Bitmap(localPath);
            }
            else if (uri.StartsWith("https://")) {
                try {
                    using HttpClient client = new();
                    byte[] data = await client.GetByteArrayAsync(uri);
                    using MemoryStream ms = new(data);
                    mod.Thumbnail = new Bitmap(ms);
                }
                catch (Exception ex) {
                    Trace.WriteLine($"""
                        Error reading thumbnail URL: '{uri}'

                        Exception:
                        {ex}
                        """);
                }
            }
            else {
                goto Default;
            }

            return;
        }

    Default:
        mod.Thumbnail = _defaultThumbnail;
    }

    private static async Task<Mod> ImportAsync(string arg)
    {
        Mod mod = await Mod.FromPath(arg);
        if (!ProfileManager.Shared.Mods.TryInsert(mod)) {
            ProfileManager.Shared.Current.Mods.Add(mod);
        }

        mod.RefreshOptions();
        return mod;
    }

    private static bool TryInsert<T>(this ObservableCollection<T> items, T item) where T : IModItem
    {
        if (items.FirstOrDefault(x => x.Id == item.Id) is T match && items.IndexOf(match) is int index && index > -1) {
            items.Insert(index, item);
            return true;
        }

        items.Add(item);
        return false;
    }
}
