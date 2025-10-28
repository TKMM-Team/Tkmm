using Avalonia.Controls;
using MenuFactory.Abstractions;
using Tkmm.Core;
using Tkmm.Models.MenuModels;

namespace Tkmm.Extensions;

public static class MenuExtension
{
    public static void ConfigureMenu(this IMenuFactory factory)
    {
        factory.AddMenuGroup<FileMenuModel>();
        factory.AddMenuGroup<ModMenuModel>();
        factory.AddMenuGroup<ToolsMenuModel>();
#if DEBUG
        factory.AddMenuGroup<DebugMenuModel>();
#endif
        factory.AddMenuGroup<HelpMenuModel>();
        
        if (!Config.Shared.ShowAdvancedSettings) {
            FilterAdvancedItems(factory);
        }
    }
    
    private static void FilterAdvancedItems(IMenuFactory factory)
    {
        var advancedHeaders = new[] {
            Locale[TkLocale.EditExportLocations_Title]
        };
        
        foreach (var parentItem in factory.Items) {
            if (((dynamic)parentItem).Items is not { } items) {
                continue;
            }
            
            foreach (var childItem in items) {
                if (childItem is not MenuItem) {
                    continue;
                }
                
                var header = (string)childItem.Header;
                if (header != null && advancedHeaders.Contains(header)) {
                    childItem.IsVisible = false;
                }
            }
        }
    }
}