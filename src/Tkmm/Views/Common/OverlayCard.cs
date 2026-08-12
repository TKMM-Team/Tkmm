using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Tkmm.Views.Common;

public class OverlayCard : ContentControl
{
    public static readonly StyledProperty<double> CardMaxWidthProperty =
        AvaloniaProperty.Register<OverlayCard, double>(nameof(CardMaxWidth), double.PositiveInfinity);

    public static readonly StyledProperty<double> CardMinWidthProperty =
        AvaloniaProperty.Register<OverlayCard, double>(nameof(CardMinWidth), 0);

    public static readonly StyledProperty<Thickness> CardPaddingProperty =
        AvaloniaProperty.Register<OverlayCard, Thickness>(nameof(CardPadding), new Thickness(25));

    public static readonly StyledProperty<Thickness> CardMarginProperty =
        AvaloniaProperty.Register<OverlayCard, Thickness>(nameof(CardMargin), new Thickness(200, 0));

    public static readonly StyledProperty<HorizontalAlignment> CardHorizontalAlignmentProperty =
        AvaloniaProperty.Register<OverlayCard, HorizontalAlignment>(nameof(CardHorizontalAlignment), HorizontalAlignment.Center);

    protected override Type StyleKeyOverride => typeof(OverlayCard);

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
}