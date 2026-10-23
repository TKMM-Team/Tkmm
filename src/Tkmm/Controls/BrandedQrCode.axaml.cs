using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Net.Codecrete.QrCodeGenerator;

namespace Tkmm.Controls;

public partial class BrandedQrCode : UserControl
{
    public static readonly IBrush BrandBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0x86, 0xC5));

    public static readonly StyledProperty<string?> UrlProperty =
        AvaloniaProperty.Register<BrandedQrCode, string?>(nameof(Url));

    private const int BORDER_MODULES = 6;
    private const int MODULE_SCALE = 10;
    private const int FINDER_SIZE = 7;
    private const double CENTER_CLEAR_RATIO = 0.17;
    private const double FINDER_CORNER_RADIUS = 24;

    private RenderTargetBitmap? _bitmap;

    public BrandedQrCode()
    {
        InitializeComponent();
    }

    public string? Url {
        get => GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UrlProperty) {
            RenderQr();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RenderQr();
    }

    private void RenderQr()
    {
        if (string.IsNullOrWhiteSpace(Url)) {
            QrImage.Source = null;
            DisposeBitmap();
            return;
        }

        var qr = QrCode.EncodeText(Url, QrCode.Ecc.High);
        var modules = qr.Size;
        var pixelSize = (modules + BORDER_MODULES * 2) * MODULE_SCALE;
        var center = (modules - 1) / 2.0;
        var clearRadius = modules * CENTER_CLEAR_RATIO;

        var bitmap = new RenderTargetBitmap(new PixelSize(pixelSize, pixelSize), new Vector(96, 96));
        using (var context = bitmap.CreateDrawingContext(clear: true)) {
            for (var y = 0; y < modules; y++) {
                for (var x = 0; x < modules; x++) {
                    if (!qr.GetModule(x, y) || IsFinderModule(x, y, modules) || IsInCenterClear(x, y, center, clearRadius)) {
                        continue;
                    }

                    context.FillRectangle(BrandBrush, ModuleRect(x, y));
                }
            }

            DrawFinder(context, 0, 0);
            DrawFinder(context, modules - FINDER_SIZE, 0);
            DrawFinder(context, 0, modules - FINDER_SIZE);
        }

        DisposeBitmap();
        _bitmap = bitmap;
        QrImage.Source = bitmap;
    }

    private void DisposeBitmap()
    {
        if (_bitmap is null) {
            return;
        }

        QrImage.Source = null;
        _bitmap.Dispose();
        _bitmap = null;
    }

    private static void DrawFinder(DrawingContext context, int moduleX, int moduleY)
    {
        var outer = ModuleRect(moduleX, moduleY, FINDER_SIZE, FINDER_SIZE);
        var gap = ModuleRect(moduleX + 1, moduleY + 1, FINDER_SIZE - 2, FINDER_SIZE - 2);
        var core = ModuleRect(moduleX + 2, moduleY + 2, 3, 3);

        context.DrawRectangle(BrandBrush, null, outer, FINDER_CORNER_RADIUS, FINDER_CORNER_RADIUS);
        context.DrawRectangle(Brushes.White, null, gap, FINDER_CORNER_RADIUS, FINDER_CORNER_RADIUS);
        context.DrawRectangle(BrandBrush, null, core, FINDER_CORNER_RADIUS, FINDER_CORNER_RADIUS);
    }

    private static Rect ModuleRect(int moduleX, int moduleY, int widthModules = 1, int heightModules = 1)
        => new(
            (moduleX + BORDER_MODULES) * MODULE_SCALE,
            (moduleY + BORDER_MODULES) * MODULE_SCALE,
            widthModules * MODULE_SCALE,
            heightModules * MODULE_SCALE);

    private static bool IsFinderModule(int x, int y, int size)
        => IsInFinder(x, y, 0, 0)
           || IsInFinder(x, y, size - FINDER_SIZE, 0)
           || IsInFinder(x, y, 0, size - FINDER_SIZE);

    private static bool IsInFinder(int x, int y, int originX, int originY)
        => x >= originX && x < originX + FINDER_SIZE && y >= originY && y < originY + FINDER_SIZE;

    private static bool IsInCenterClear(int x, int y, double center, double radius)
    {
        var dx = x - center;
        var dy = y - center;
        return dx * dx + dy * dy <= radius * radius;
    }
}