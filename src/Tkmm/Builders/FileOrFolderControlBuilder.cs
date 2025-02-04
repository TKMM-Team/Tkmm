using System.Reflection;
using Avalonia.Data;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using ConfigFactory.Generics;
using Tkmm.Core.Models;

namespace Tkmm.Builders;

public class FileOrFolderControlBuilder : ControlBuilder<FileOrFolderControlBuilder>
{
    public override object Build(IConfigModule context, PropertyInfo propertyInfo)
    {
        FileOrFolderEdit result = new() {
            DataContext = context,
            [!FileOrFolderEdit.ValueProperty] = new Binding(propertyInfo.Name)
        };
        
        if (propertyInfo.GetCustomAttribute<BrowserConfigAttribute>() is BrowserConfigAttribute browserConfig) {
            result.FileBrowserFilter = browserConfig.Filter;
            result.FileBrowserSuggestedFileName = browserConfig.SuggestedFileName;
            result.BrowserTitle = browserConfig.Title;
        }

        return result;
    }

    public override bool IsValid(Type type)
    {
        return type == typeof(FileOrFolder);
    }
}