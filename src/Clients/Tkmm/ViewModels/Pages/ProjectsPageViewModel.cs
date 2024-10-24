using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Core;
using Tkmm.Core.Studio;

namespace Tkmm.ViewModels.Pages;

public sealed partial class ProjectsPageViewModel : ObservableObject
{
    public static TkProjectManager Manager => TKMM.ProjectManager;
}