using System.Reflection;
using Avalonia.Data;
using Avalonia.Layout;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using ConfigFactory.Generics;
using Tkmm.Controls;
using Tkmm.Core.Attributes;
using Tkmm.Core.Models;

namespace Tkmm.Builders;

internal partial class PathCollectionControlBuilder : ControlBuilder<PathCollectionControlBuilder>
{
    public override object Build(IConfigModule context, PropertyInfo propertyInfo)
    {
        var type = PathType.FileOrFolder;
        if (propertyInfo.GetCustomAttribute<PathCollectionOptionsAttribute>() is { } options) {
            type = options.Type;
        }
        
        PathCollectionEditor editor = new(type) {
            VerticalAlignment = VerticalAlignment.Top,
            DataContext = context,
            [!PathCollectionEditor.ValueProperty] = new Binding(propertyInfo.Name)
        };
        
        if (propertyInfo.GetCustomAttribute<BrowserConfigAttribute>() is { } browserConfig) {
            editor.FileBrowserFilter = browserConfig.Filter;
            editor.FileBrowserSuggestedFileName = browserConfig.SuggestedFileName;
            editor.BrowserTitle = browserConfig.Title;
        }

        return editor;
    }

    public override bool IsValid(Type type)
    {
        return type == typeof(PathCollection);
    }
}
