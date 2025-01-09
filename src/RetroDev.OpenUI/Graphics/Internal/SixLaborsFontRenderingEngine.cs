using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RetroDev.OpenUI.Core.Internal;

internal class SixLaborsFontRenderingEngine : IFontRenderingEngine
{
    private static FontCollection? _fontCollection;
    private static FontFamily? _fontFamily;
    private static Font? _font;

    public RgbaImage ConvertTextToRgbaImage(string text, int fontSize, Graphics.Color textColor)
    {
        // Locate the font file or use a fallback
        string fontPath = File.Exists("C:\\Windows\\Fonts\\arial.ttf")
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

        int width = (int)Math.Ceiling(textBounds.Width);
        int height = (int)Math.Ceiling(textBounds.Height);

        // Create the image with transparent background
        using var image = new Image<Rgba32>(width, height);
        var rgbaTextColor = new Rgba32(textColor.Red, textColor.Green, textColor.Blue, textColor.Alpha);

        // Adjust the drawing origin to align the text at the top-left
        float offsetX = -textBounds.Left;
        float offsetY = -textBounds.Top;

        // Render the text
        image.Mutate(ctx =>
        {
            ctx.Clear(Color.Transparent);
            ctx.DrawText(text, _font, rgbaTextColor, new PointF(offsetX, offsetY));
        });

        // Extract raw RGBA pixel data
        var pixelData = new byte[width * height * 4];
        image.CopyPixelDataTo(pixelData);

        return new RgbaImage(pixelData, width, height);
    }
}
