using Avalonia.Controls;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;
public partial class GameBananaPageView : UserControl
{
    public GameBananaPageView()
    {
        InitializeComponent();
        DataContext = new GameBananaPageViewModel();
    }
}
