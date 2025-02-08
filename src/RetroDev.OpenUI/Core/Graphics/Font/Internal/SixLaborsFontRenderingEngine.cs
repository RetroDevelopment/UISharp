using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RetroDev.OpenUI.Core.Graphics.Font.Internal;

internal class SixLaborsFontRenderingEngine : IFontRenderingEngine
{
    private static FontCollection? _fontCollection;
    private static FontFamily? _fontFamily;
    private static SixLabors.Fonts.Font? _font;

    public RgbaImage ConvertTextToRgbaImage(string text, int fontSize, Color textColor)
    {
        // Locate the font file or use a fallback
        var fontPath = File.Exists("C:\\Windows\\Fonts\\arial.ttf")
            ? "C:\\Windows\\Fonts\\arial.ttf"
            : "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";

        // Load the font
        if (_font == null)
        {
            _fontCollection = new FontCollection();
            _fontFamily = _fontCollection.Add(fontPath);
            _font = _fontFamily.Value.CreateFont(fontSize, FontStyle.Regular);
        }

        // Measure the size of the rendered text
        var textOptions = new RichTextOptions(_font)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        var textBounds = TextMeasurer.MeasureBounds(text, textOptions);

        var width = (int)Math.Ceiling(textBounds.Width);
        var height = (int)Math.Ceiling(textBounds.Height);

        // Create the image with transparent background
        using var image = new Image<Rgba32>(width, height);
        var rgbaTextColor = new Rgba32(textColor.RedComponent, textColor.GreenComponent, textColor.BlueComponent, textColor.AlphaComponent);

        // Adjust the drawing origin to align the text at the top-left
        var offsetX = -textBounds.Left;
        var offsetY = -textBounds.Top;

        // Render the text
        image.Mutate(ctx =>
        {
            ctx.Clear(SixLabors.ImageSharp.Color.Transparent);
            ctx.DrawText(text, _font, rgbaTextColor, new PointF(offsetX, offsetY));
        });

        // Extract raw RGBA pixel data
        var pixelData = new byte[width * height * 4];
        image.CopyPixelDataTo(pixelData);

        return new RgbaImage(pixelData, width, height);
    }
}
