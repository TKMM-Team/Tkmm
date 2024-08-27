using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Diagnostics;
using Tkmm.Core;
using Tkmm.Core.Components;
using Tkmm.Core.Exceptions;
using Tkmm.Core.Generics;
using Tkmm.Core.Helpers;
using Tkmm.Core.Models.Mods;

namespace Tkmm.Helpers;

public delegate Task<Mod> CreateModDelegate<T>(T input, out string sourceFolder);

public static class ModHelper
{
    private static Bitmap? _defaultThumbnail;

    static ModHelper()
    {
        Mod.ResolveThumbnail = ResolveThumbnail;
    }

    public static async Task<Mod?> Import(string arg)
    {
        return await Import(arg, ImportAsync);
    }

    public static async Task<Mod?> Import<T>(T arg, Func<T, Task<Mod>> createMod)
    {
        App.LogTkmmInfo();

        try {
            Mod result = await Task.Run(async
                () => await createMod(arg)
            );

            ProfileManager.Shared.Mods.TryInsert(result);
            ProfileManager.Shared.Current.Mods.TryInsert(result);

            ProfileManager.Shared.Current.Selected = result;

            if (result.OptionGroups.Any()) {
                App.Toast($"'{result.Name}' has configurable options!\n\nClick here to configure them.", "Configure Options",
                    NotificationType.Information, TimeSpan.FromDays(5), () => {
                        result.IsEditingOptions = true;
                        PageManager.Shared.Focus(Page.Home);
                    });
            }

            AppStatus.Set("Install Complete!", "fa-regular fa-circle-check", isWorkingStatus: false, temporaryStatusTime: 1.5);
            return result;
        }
        catch (PackageException ex) {
            if (Directory.Exists(ex.SystemModFolder)) {
                try {
                    Directory.Delete(ex.SystemModFolder, recursive: true);
                }
                catch (Exception deleteException) {
                    AppLog.Log(deleteException);
                }
            }

            App.ToastError(ex.InnerException ?? ex);
        }
        catch (Exception ex) {
            App.ToastError(ex);
        }

        AppStatus.Set("Install Failed!", "fa-regular fa-circle-xmark", isWorkingStatus: false, temporaryStatusTime: 1.5);
        return null;
    }

    public static async Task ResolveThumbnail(IModItem item, bool useDefaultThumbnail = false)
    {
        if (item is Mod mod) {
            foreach (ModOptionGroup modOptionGroup in mod.OptionGroups) {
                await ResolveThumbnail(modOptionGroup);
            }
        }

        if (item is ModOptionGroup group) {
            foreach (ModOption option in group.Options) {
                await ResolveThumbnail(option);
            }
        }

        if (item.Thumbnail is not null) {
            return;
        }

        if (item.ThumbnailUri is string uri) {
            string localPath = Path.Combine(item.SourceFolder, uri);
            if (File.Exists(localPath)) {
                item.Thumbnail = new Bitmap(localPath);
            }
            else if (uri.StartsWith("https://")) {
                try {
                    using HttpClient client = new();
                    byte[] data = await client.GetByteArrayAsync(uri);
                    using MemoryStream ms = new(data);
                    item.Thumbnail = new Bitmap(ms);
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
        if (_defaultThumbnail is null) {
            using Stream stream = AssetLoader.Open(new("avares://Tkmm/Assets/DefaultThumbnail.jpg"));
            _defaultThumbnail = new Bitmap(stream);
        }

        if (useDefaultThumbnail) {
            item.Thumbnail = _defaultThumbnail;
        }
    }

    private static async Task<Mod> ImportAsync(string arg)
    {
        AppStatus.Set($"Installing '{arg}'", "fa-solid fa-download", isWorkingStatus: true);

        Mod mod = await Mod.FromPath(arg);
        mod.RefreshOptions();
        return mod;
    }
}
