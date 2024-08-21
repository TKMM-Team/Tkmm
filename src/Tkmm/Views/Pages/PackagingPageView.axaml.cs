using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Tkmm.Attributes;
using Tkmm.Helpers;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

[Page(Page.Tools, "Mod developer tools", Symbol.CodeHTML, "TKCL Packager")]
public partial class PackagingPageView : UserControl
{
    public PackagingPageView()
    {
        InitializeComponent();
        DataContext = new PackagingPageViewModel();
    }
}