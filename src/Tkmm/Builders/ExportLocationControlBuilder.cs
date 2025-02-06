#if !SWITCH

using System.Reflection;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Core;
using ConfigFactory.Generics;
using Tkmm.Core.Models;
using Tkmm.Helpers;

namespace Tkmm.Builders;

internal class ExportLocationControlBuilder : ControlBuilder<ExportLocationControlBuilder>
{
    public override object Build(IConfigModule context, PropertyInfo propertyInfo)
    {
        return new Button {
            Content = Locale[TkLocale.Action_Edit],
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Command = new AsyncRelayCommand<ExportLocations>(
                async exportLocations => {
                    if (exportLocations is null) {
                        return;
                    }
                    
                    await ExportLocationsHelper.OpenEditorDialog(exportLocations);
                }),
            DataContext = context,
            [!Button.CommandParameterProperty] = new Binding(propertyInfo.Name)
        };
    }

    public override bool IsValid(Type type)
    {
        return type == typeof(ExportLocations);
    }
}
#endif