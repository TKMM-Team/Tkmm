using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class GameBananaModPageView : UserControl
{
    private Border? _imageContainer;
    private int _previousImageIndex = -1;

    public GameBananaModPageView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GameBananaModPageViewModel viewModel)
        {
            _imageContainer = this.FindControl<Border>("ImageContainer");
            if (_imageContainer != null)
            {
                _imageContainer.RenderTransform = new TranslateTransform();
            }
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameBananaModPageViewModel.SelectedImageIndex) && _imageContainer?.RenderTransform is TranslateTransform transform)
        {
            var viewModel = (GameBananaModPageViewModel)sender!;
            
            if (_previousImageIndex >= 0)
            {
                // Simple slide animation
                var direction = viewModel.SelectedImageIndex > _previousImageIndex ? 100 : -100;
                transform.X = direction;
                
                // Animate to center
                _ = Task.Run(async () =>
                {
                    for (int i = 0; i < 20; i++)
                    {
                        await Task.Delay(15);
                        var progress = i / 19.0;
                        var eased = 1 - Math.Pow(1 - progress, 3); // Cubic ease out
                        var x = direction * (1 - eased);
                        
                        Dispatcher.UIThread.Post(() => transform.X = x);
                    }
                });
            }
            
            _previousImageIndex = viewModel.SelectedImageIndex;
        }
    }
}