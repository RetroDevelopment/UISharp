using SkiaSharp;
using System.Text;

namespace RetroDev.OpenUI.Core.Internal;

internal class SvgRenderingEngine : ISVGRenderingEngine
{
    public RgbaImage ConvertSvgToRgba(string svgContent)
    {
        byte[] rgba;
        int width = 100;
        int height = 100;

        // Load the SVG content from the string
        var svg = new SkiaSharp.Extended.Svg.SKSvg();

        // Use a memory stream to load the SVG content from the string
        var svgStream = new MemoryStream(Encoding.UTF8.GetBytes(svgContent));
        var svgPicture = svg.Load(svgStream);

        if (svgPicture == null)
            throw new ArgumentException("Unable to load SVG content.");

        // Set up the bitmap with the specified width and height
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);

        // Clear the canvas (optional, in case the SVG doesn't cover the entire canvas)
        canvas.Clear(SKColors.Transparent);

        // Scale the SVG to fit the desired width and height
        var scaleX = (float)width / svgPicture.CullRect.Width;
        var scaleY = (float)height / svgPicture.CullRect.Height;
        canvas.Scale(scaleX, scaleY);

        // Anti alias
        var paint = new SKPaint
        {
            IsAntialias = true
        };

        // Render the SVG onto the canvas
        canvas.DrawPicture(svgPicture, paint);

        // Extract the RGBA pixel data from the bitmap
        rgba = bitmap.Bytes;

        return new RgbaImage(rgba, width, height);
    }
}
