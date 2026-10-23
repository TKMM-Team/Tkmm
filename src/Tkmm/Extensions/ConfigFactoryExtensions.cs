using ConfigFactory;
using ConfigFactory.Core;
using ConfigFactory.Models;
using Tkmm.Core;
using TkSharp.Core;

namespace Tkmm.Extensions;

public static class ConfigFactoryExtensions
{
    public static void AppendAndValidate<T>(this ConfigPageModel settingsModel, ref bool isValid) where T : ConfigModule<T>, new()
    {
        isValid = ConfigModule<T>.Shared.Validate(out var _, out var target);

        if (!isValid && target?.Attribute is not null) {
            settingsModel.SelectedGroup = settingsModel.Categories
                .Where(x => x.Header == target.Attribute.Category)
                .SelectMany(x => x.Groups)
                .FirstOrDefault(x => x.Header == target.Attribute.Group);

            TkStatus.Set(Locale[TkLocale.Error_InvalidSetting, target.Property.Name], TkIcons.WARNING);
        }

        settingsModel.Append<T>();

        List<string> headersToHide = [];

        if (!Config.Shared.ShowAdvancedSettings) {
            headersToHide.AddRange([
                Locale["Config_MergeAllMalsLocales"],
                Locale["Config_ExportLocations"],
                Locale["TkConfig_KeysFolderPath"],
                Locale["TkConfig_PackagedBaseGamePaths"],
                Locale["TkConfig_GameUpdateFilePaths"],
                Locale["TkConfig_SdCardRootPath"],
                Locale["TkConfig_GameDumpFolderPaths"],
                Locale["TkConfig_NandFolderPaths"]
            ]);
        }

#if !SWITCH
        if (Config.Shared.TkmmMode.IsSwitch) {
            headersToHide.Add(Locale["Config_MergeOutputFolder"]);
        }

        if (Config.Shared.TkmmMode.IsEmulator) {
            headersToHide.Add(Locale["Config_SwitchFirmwareVersion"]);
        }
#endif

        if (headersToHide.Count == 0) {
            return;
        }

        foreach (var category in settingsModel.Categories) {
            foreach (var group in category.Groups) {
                var itemsToRemove = group.Items
                    .Where(item => headersToHide.Contains(item.Header))
                    .ToList();

                foreach (var item in itemsToRemove) {
                    group.Items.Remove(item);
                }
            }
        }
    }
}