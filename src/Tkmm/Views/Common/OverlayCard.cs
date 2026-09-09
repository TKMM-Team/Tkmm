using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
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

        Easing easing = opening ? new CubicEaseOut() : new CubicEaseIn();
        EnsureTransitions(_backdrop, scale, easing);

        _backdrop.Opacity = toOpacity;
        _card.Opacity = toOpacity;
        scale.ScaleX = toScale;
        scale.ScaleY = toScale;

        await Task.Delay(AnimationDuration);
    }

    private void EnsureTransitions(Border backdrop, ScaleTransform scale, Easing easing)
    {
        backdrop.Transitions = [
            new DoubleTransition {
                Property = OpacityProperty,
                Duration = AnimationDuration,
                Easing = easing
            }
        ];

        _card!.Transitions = [
            new DoubleTransition {
                Property = OpacityProperty,
                Duration = AnimationDuration,
                Easing = easing
            }
        ];

        scale.Transitions = [
            new DoubleTransition {
                Property = ScaleTransform.ScaleXProperty,
                Duration = AnimationDuration,
                Easing = easing
            },
            new DoubleTransition {
                Property = ScaleTransform.ScaleYProperty,
                Duration = AnimationDuration,
                Easing = easing
            }
        ];
    }
}
