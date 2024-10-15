using System.ComponentModel;
using Avalonia.Controls.PanAndZoom;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tkmm.Abstractions;
using Tkmm.Core;

namespace Tkmm.ViewModels.Pages;

public partial class ShopParamPageViewModel : ObservableObject
{
    private readonly ZoomBorder _zoomBorder;

    public static ITkShopManager ShopManager => TKMM.ShopManager;

    public ShopParamPageViewModel(ZoomBorder zoomBorder)
    {
        _zoomBorder = zoomBorder;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (ShopManager is not INotifyPropertyChanged vm) {
            return;
        }
        
        vm.PropertyChanged += (_, args) => {
            if (args.PropertyName is not nameof(ITkShopManager.Selected)) {
                return;
            }
            
            GotoSelected();
        };
    }

    [RelayCommand]
    private static async Task MoveUp()
    {
        TKMM.ShopManager.MoveUp();
        await TKMM.ShopManager.Save();
    }

    [RelayCommand]
    private static async Task MoveDown()
    {
        TKMM.ShopManager.MoveDown();
        await TKMM.ShopManager.Save();
    }

    [RelayCommand]
    private void ResetMap()
    {
        _zoomBorder.ResetMatrix();
    }

    [RelayCommand]
    private void GotoSelected()
    {
        ITkShop? selected;
        if ((selected = ShopManager.Selected) is null) {
            return;
        }

        const int zoom = 6;
        _zoomBorder.Zoom(zoom,
            selected.Coordinates.X + selected.Coordinates.X / zoom + 6000,
            selected.Coordinates.Y + selected.Coordinates.Y / zoom + 5000
        );
    }
}