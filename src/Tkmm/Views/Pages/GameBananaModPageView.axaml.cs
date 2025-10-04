using Avalonia.Controls;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;
using Avalonia.Media;
using Tkmm.ViewModels.Pages;

namespace Tkmm.Views.Pages;

public partial class GameBananaModPageView : UserControl
{
    private Image? _mainImage;
    private TranslateTransform? _imageTransform;
    private int _previousImageIndex = -1;
    private CancellationTokenSource? _animationCancellation;

    public GameBananaModPageView()
    {
        InitializeComponent();
#if !SWITCH
        DataContextChanged += OnDataContextChanged;
#endif
    }
#if !SWITCH
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not GameBananaModPageViewModel viewModel) {
            return;
        }
        
        _mainImage = this.FindControl<Image>("MainImage");
        
        if (_mainImage?.RenderTransform is TranslateTransform transform) {
            _imageTransform = transform;
        }
        
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(GameBananaModPageViewModel.SelectedImageIndex) ||
            _mainImage == null || _imageTransform == null) {
            return;
        }
        var viewModel = (GameBananaModPageViewModel)sender!;
            
        if (_previousImageIndex >= 0) {
            _animationCancellation?.Cancel();
            _animationCancellation?.Dispose();
            _animationCancellation = new CancellationTokenSource();
                
            var direction = viewModel.SelectedImageIndex > _previousImageIndex ? 200 : -200;
                
            var animation = new Animation {
                Duration = TimeSpan.FromMilliseconds(400),
                Easing = new CubicEaseOut(),
                Children =
                {
                    new KeyFrame {
                        Cue = new Cue(0.0),
                        Setters = { new Setter(TranslateTransform.XProperty, (double)direction) }
                    },
                    new KeyFrame {
                        Cue = new Cue(1.0),
                        Setters = { new Setter(TranslateTransform.XProperty, 0.0) }
                    }
                }
            };
                
            _ = animation.RunAsync(_mainImage, _animationCancellation.Token);
        }
            
        _previousImageIndex = viewModel.SelectedImageIndex;
    }
#endif
}