using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Diagnostics;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Helpers;
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

            ProfileManager.Shared.Current.Selected = result;

            if (result.OptionGroups.Any()) {
                App.Toast($"'{result.Name}' has options, click here or go to the home page to configure them.", "Configure Options",
                    NotificationType.Information, TimeSpan.FromSeconds(3), () => {
                        result.IsEditingOptions = true;
                        PageManager.Shared.Focus(Page.Home);
                    });
            }

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
        ProfileManager.Shared.Current.Mods.TryInsert(mod);
        ProfileManager.Shared.Mods.TryInsert(mod);

        mod.RefreshOptions();
        return mod;
    }
}
