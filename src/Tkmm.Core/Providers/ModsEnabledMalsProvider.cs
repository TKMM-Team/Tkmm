using MessageStudio.Formats.BinaryText;
using SarcLibrary;
using TkSharp.Core;
using TkSharp.Core.Models;

namespace Tkmm.Core.Providers;

public static class ModsEnabledMalsProvider
{
    private static readonly Dictionary<string, string> LanguageMappings = new()
    {
        ["CNzh"] = "模组已启用",
        ["EUde"] = "Mods Aktiviert",
        ["EUen"] = "Mods Enabled",
        ["EUes"] = "Mods Habilitados",
        ["EUfr"] = "Mods Activés",
        ["EUit"] = "Mod Attivati",
        ["EUnl"] = "Mods Ingeschakeld",
        ["EUru"] = "Моды Включены",
        ["JPja"] = "モード有効",
        ["KRko"] = "모드 활성화됨",
        ["TWzh"] = "模組已啟用",
        ["USen"] = "Mods Enabled",
        ["USes"] = "Mods Habilitados",
        ["USfr"] = "Mods Activés"
    };
    
    public static TkChangelog? CreateDefaultMalsChangelog(string locale)
    {
        if (locale.Length < 4 || !LanguageMappings.TryGetValue(locale, out var localizedText)) {
            return null;
        }

        try {
            Msbt changelog = new() { ["0003"] = new MsbtEntry { Text = "TKMM ~ " + localizedText } };

            using var msbtStream = new MemoryStream();
            changelog.WriteBinary(msbtStream);
            var msbtData = msbtStream.ToArray();

            var sarc = new Sarc { ["LayoutMsg/Title_00.msbt"] = msbtData };

            var sarcStream = new MemoryStream();
            sarc.Write(sarcStream);
            sarcStream.Position = 0;

            var malsFile = $"Mals/{locale}.Product.sarc";
            
            return new TkChangelog {
                BuilderVersion = 100,
                GameVersion = 121,
                MalsFiles = { malsFile },
                Source = new StreamSystemSource($"romfs/{malsFile}", sarcStream)
            };
        }
        catch {
            return null;
        }
    }

    private sealed class StreamSystemSource(string path, Stream stream) : ITkSystemSource
    {
        public Stream OpenRead(string path1) 
        {
            if (path1 != path) {
                throw new FileNotFoundException();
            }
            stream.Position = 0;
            return stream;
        }
        public bool Exists(string path1) => path1 == path;
        public ITkSystemSource GetRelative(string _) => this;
    }
}
