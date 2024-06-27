using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Core;
using ConfigFactory.Generics;
using FluentAvalonia.UI.Controls;
using System.Reflection;
using Tkmm.Builders.Controls;
using Tkmm.Core.Models;

namespace Tkmm.Builders;

internal partial class ExportLocationControlBuilder : ControlBuilder<ExportLocationControlBuilder>
{
    public override object? Build(IConfigModule context, PropertyInfo propertyInfo)
    {
        return new Button {
            Content = "Edit",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Command = EditCommand,
            DataContext = context,
            [!Button.CommandParameterProperty] = new Binding(propertyInfo.Name)
        };
    }

    public override bool IsValid(Type type)
    {
        return type == typeof(ExportLocationCollection);
    }

    [RelayCommand]
    private static async Task Edit(ExportLocationCollection source)
    {
        TaskDialog dialog = new() {
            DataContext = source,
            Buttons = [
                TaskDialogButton.CloseButton
            ],
            Title = "Edit Export Locations",
            Content = new ExportLocationCollectionEditor(),
            XamlRoot = App.XamlRoot
        };

        await dialog.ShowAsync();
    }
}
