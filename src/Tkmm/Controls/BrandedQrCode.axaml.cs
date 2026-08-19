using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Net.Codecrete.QrCodeGenerator;

namespace Tkmm.Controls;

public partial class BrandedQrCode : UserControl
{
    public static readonly StyledProperty<string?> UrlProperty =
        AvaloniaProperty.Register<BrandedQrCode, string?>(nameof(Url));

    private static readonly StyledProperty<Color> ModuleColorProperty =
        AvaloniaProperty.Register<BrandedQrCode, Color>(nameof(ModuleColor), Colors.Black);

    private const int BORDER_MODULES = 4;
    private const int MODULE_SCALE = 8;

    public BrandedQrCode()
    {
        InitializeComponent();
    }

    public string? Url {
        get => GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    public Color ModuleColor {
        get => GetValue(ModuleColorProperty);
        set => SetValue(ModuleColorProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UrlProperty || change.Property == ModuleColorProperty) {
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
            return;
        }

        var qr = QrCode.EncodeText(Url, QrCode.Ecc.High);
        var pixelSize = (qr.Size + BORDER_MODULES * 2) * MODULE_SCALE;
        var moduleColor = GetModuleColor();

        var bitmap = new WriteableBitmap(
            new PixelSize(pixelSize, pixelSize),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul);

        using (var frameBuffer = bitmap.Lock()) {
            var stride = frameBuffer.RowBytes / 4;
            var pixels = new uint[pixelSize * stride];

            Array.Fill(pixels, Colors.White.ToUInt32());

            for (var y = 0; y < qr.Size; y++) {
                for (var x = 0; x < qr.Size; x++) {
                    if (!qr.GetModule(x, y)) {
                        continue;
                    }

                    var startX = (x + BORDER_MODULES) * MODULE_SCALE;
                    var startY = (y + BORDER_MODULES) * MODULE_SCALE;

                    for (var dy = 0; dy < MODULE_SCALE; dy++) {
                        var row = (startY + dy) * stride;
                        for (var dx = 0; dx < MODULE_SCALE; dx++) {
                            pixels[row + startX + dx] = moduleColor;
                        }
                    }
                }
            }

            CopyPixels(frameBuffer.Address, pixels);
        }

        QrImage.Source = bitmap;
    }

    private static unsafe void CopyPixels(IntPtr destination, uint[] pixels)
    {
        fixed (uint* source = pixels) {
            Buffer.MemoryCopy(source, (void*)destination, pixels.Length * sizeof(uint), pixels.Length * sizeof(uint));
        }
    }

    private uint GetModuleColor() => ModuleColor.ToUInt32();
}