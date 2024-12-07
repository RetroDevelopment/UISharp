using RetroDev.OpenUI.Graphics;
using SkiaSharp;

namespace RetroDev.OpenUI.Core.Internal;

internal class SkiaFontRenderingEngine : IFontRenderingEngine
{
    public RgbaImage ConvertTextToRgbaImage(string text, int fontSize, Color textColor)
    {
        // Load the font from a file (using Arial as an example)
        using var typeface = SKTypeface.FromFile("C:\\Windows\\Fonts\\arial.ttf");

        // Set up paint properties for text rendering
        using var paint = new SKPaint
        {
            Typeface = typeface,
            TextSize = fontSize,
            IsAntialias = true,
            Color = new SKColor(textColor.Red, textColor.Green, textColor.Blue, textColor.Alpha)
        };

        // Measure text dimensions
        var textBounds = new SKRect();
        paint.MeasureText(text, ref textBounds);

        // Create a bitmap with the necessary size and RGBA format
        int width = (int)Math.Ceiling(textBounds.Width);
        int height = (int)Math.Ceiling(textBounds.Height);
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

        // Render text onto a transparent canvas
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent); // Ensures the background is fully transparent
            canvas.DrawText(text, -textBounds.Left, -textBounds.Top, paint);
        }

        // Extract raw RGBA pixel data
        var pixels = bitmap.Bytes;

        return new RgbaImage(pixels, bitmap.Width, bitmap.Height);
    }
}
