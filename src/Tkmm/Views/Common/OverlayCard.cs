using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Tkmm.Views.Common;

public class OverlayCard : ContentControl
{
    private const double CLOSED_SCALE = 0.94;
    private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(320);

    public static readonly StyledProperty<double> CardMaxWidthProperty =
        AvaloniaProperty.Register<OverlayCard, double>(nameof(CardMaxWidth), double.PositiveInfinity);

    public static readonly StyledProperty<double> CardMinWidthProperty =
        AvaloniaProperty.Register<OverlayCard, double>(nameof(CardMinWidth));

    public static readonly StyledProperty<double> CardMaxHeightProperty =
        AvaloniaProperty.Register<OverlayCard, double>(nameof(CardMaxHeight), double.PositiveInfinity);

    public static readonly StyledProperty<double> CardMinHeightProperty =
        AvaloniaProperty.Register<OverlayCard, double>(nameof(CardMinHeight));

    public static readonly StyledProperty<Thickness> CardPaddingProperty =
        AvaloniaProperty.Register<OverlayCard, Thickness>(nameof(CardPadding), new Thickness(25));

    public static readonly StyledProperty<Thickness> CardMarginProperty =
        AvaloniaProperty.Register<OverlayCard, Thickness>(nameof(CardMargin), new Thickness(200, 0));

    public static readonly StyledProperty<HorizontalAlignment> CardHorizontalAlignmentProperty =
        AvaloniaProperty.Register<OverlayCard, HorizontalAlignment>(nameof(CardHorizontalAlignment), HorizontalAlignment.Center);

    private Border? _backdrop;
    private Border? _card;
    private CancellationTokenSource? _animationCts;

    protected override Type StyleKeyOverride => typeof(OverlayCard);

    public static OverlayCard Sized(double width, double height) => new() {
        CardPadding = new Thickness(0),
        CardMargin = new Thickness(0),
        CardMinWidth = width,
        CardMaxWidth = width,
        CardMinHeight = height,
        CardMaxHeight = height
    };

    public double CardMaxWidth
    {
        get => GetValue(CardMaxWidthProperty);
        set => SetValue(CardMaxWidthProperty, value);
    }

    public double CardMinWidth
    {
        get => GetValue(CardMinWidthProperty);
        set => SetValue(CardMinWidthProperty, value);
    }

    public double CardMaxHeight
    {
        get => GetValue(CardMaxHeightProperty);
        set => SetValue(CardMaxHeightProperty, value);
    }

    public double CardMinHeight
    {
        get => GetValue(CardMinHeightProperty);
        set => SetValue(CardMinHeightProperty, value);
    }

    public Thickness CardPadding
    {
        get => GetValue(CardPaddingProperty);
        set => SetValue(CardPaddingProperty, value);
    }

    public Thickness CardMargin
    {
        get => GetValue(CardMarginProperty);
        set => SetValue(CardMarginProperty, value);
    }

    public HorizontalAlignment CardHorizontalAlignment
    {
        get => GetValue(CardHorizontalAlignmentProperty);
        set => SetValue(CardHorizontalAlignmentProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _backdrop = e.NameScope.Find<Border>("PART_Backdrop");
        _card = e.NameScope.Find<Border>("PART_Card");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Dispatcher.UIThread.Post(() => _ = PlayOpenAsync(), DispatcherPriority.Loaded);
    }

    private Task PlayOpenAsync() => AnimateAsync(opening: true);

    public Task PlayCloseAsync() => AnimateAsync(opening: false);

    private async Task AnimateAsync(bool opening)
    {
        if (_backdrop is null || _card is null) {
            ApplyTemplate();
        }

        if (_backdrop is null || _card is null) {
            return;
        }

        var scale = _card.RenderTransform as ScaleTransform ?? new ScaleTransform(CLOSED_SCALE, CLOSED_SCALE);
        _card.RenderTransform = scale;
        _card.RenderTransformOrigin = RelativePoint.Center;

        var toOpacity = opening ? 1.0 : 0.0;
        var toScale = opening ? 1.0 : CLOSED_SCALE;

        if (Math.Abs(_backdrop.Opacity - toOpacity) < 0.01 && Math.Abs(scale.ScaleX - toScale) < 0.01) {
            return;
        }

        if (_animationCts is not null) {
            if (Dispatcher.UIThread.CheckAccess()) {
                await _animationCts.CancelAsync();
            }
            else {
                await Dispatcher.UIThread.InvokeAsync(_animationCts.Cancel);
            }

            _animationCts.Dispose();
        }

        _animationCts = new CancellationTokenSource();
        var token = _animationCts.Token;

        var fromOpacity = _backdrop.Opacity;
        var fromScale = scale.ScaleX;
        Easing easing = opening ? new CubicEaseOut() : new CubicEaseIn();

        try {
            await Task.WhenAll(
                CreateAnimation(Visual.OpacityProperty, fromOpacity, toOpacity, easing).RunAsync(_backdrop, token),
                CreateAnimation(Visual.OpacityProperty, fromOpacity, toOpacity, easing).RunAsync(_card, token),
                CreateAnimation(ScaleTransform.ScaleXProperty, fromScale, toScale, easing).RunAsync(_card, token),
                CreateAnimation(ScaleTransform.ScaleYProperty, fromScale, toScale, easing).RunAsync(_card, token));
        }
        catch (Exception ex) when (ex is OperationCanceledException or AggregateException) {
        }
    }

    private static Animation CreateAnimation(AvaloniaProperty property, double from, double to, Easing easing)
        => new() {
            Duration = AnimationDuration,
            Easing = easing,
            FillMode = FillMode.Forward,
            Children = {
                new KeyFrame {
                    Cue = new Cue(0d),
                    Setters = { new Setter(property, from) }
                },
                new KeyFrame {
                    Cue = new Cue(1d),
                    Setters = { new Setter(property, to) }
                }
            }
        };
}
