using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RetroDev.OpenUI.Core.Graphics.Fonts;

/// <summary>
/// The font rendering engine implemented using the SixLabors library.
/// </summary>
public class SixLaborsFontRenderingEngine : IFontRenderingEngine
{
    private readonly Dictionary<Font, SixLabors.Fonts.Font> _fonts = [];

    /// <inheritdoc />
    public RgbaImage ConvertTextToRgbaImage(string text, Font font, Color textColor)
    {
        if (string.IsNullOrEmpty(text)) return RgbaImage.Empty;

        if (!_fonts.ContainsKey(font))
        {
            using var stream = new MemoryStream(font.Data);
            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add(stream);
            var sixLaborsFont = fontFamily.CreateFont(font.Size, FontStyle.Regular);
            _fonts.Add(font, sixLaborsFont);
        }

        var sixLaborsLoadedFont = _fonts[font];

        // Measure the size of the rendered text
        var textOptions = new RichTextOptions(sixLaborsLoadedFont)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        var textBounds = TextMeasurer.MeasureBounds(text, textOptions);
        var advance = TextMeasurer.MeasureAdvance(text, textOptions);

        var width = (int)Math.Floor(advance.Left + advance.Width);
        // + 1 to avoid rounding errors
        var height = (int)Math.Floor(advance.Top + advance.Height) + 2;

        // Create the image with transparent background
        using var image = new Image<Rgba32>(width, height);
        var rgbaTextColor = new Rgba32(textColor.RedComponent, textColor.GreenComponent, textColor.BlueComponent, textColor.AlphaComponent);

        // Adjust the drawing origin to align the text at the top-left
        var offsetX = 0;// -textBounds.Left;
        var offsetY = 0;// -textBounds.Top;

        // Render the text
        image.Mutate(ctx =>
        {
            ctx.Clear(SixLabors.ImageSharp.Color.Transparent);
            ctx.DrawText(text, sixLaborsLoadedFont, rgbaTextColor, new PointF(offsetX, offsetY));
        });

        // Extract raw RGBA pixel data
        var pixelData = new byte[width * height * 4];
        image.CopyPixelDataTo(pixelData);

        return new RgbaImage(pixelData, width, height);
    }

    /// <inheritdoc />
    public UI.Coordinates.Size ComputeTextSize(string text, Font font)
    {
        var img = ConvertTextToRgbaImage(text, font, Color.Red);
        return new UI.Coordinates.Size(img.Width, img.Height);
    }
}
