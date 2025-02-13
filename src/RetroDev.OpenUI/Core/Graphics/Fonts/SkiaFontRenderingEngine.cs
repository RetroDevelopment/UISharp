using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace RetroDev.OpenUI.Core.Graphics.Fonts;

/// <summary>
/// The font rendering engine implemented using the SkiaSharp library.
/// </summary>
public class SkiaSharpFontRenderingEngine : IFontRenderingEngine
{
    private readonly Dictionary<Font, SKTypeface> _fonts = new();

    /// <inheritdoc />
    public RgbaImage ConvertTextToRgbaImage(string text, Font font, Color textColor)
    {
        if (string.IsNullOrEmpty(text)) return RgbaImage.Empty;

        if (!_fonts.ContainsKey(font))
        {
            using var stream = new MemoryStream(font.Data);
            var typeface = SKTypeface.FromStream(stream);
            _fonts[font] = typeface;
        }

        var skTypeface = _fonts[font];

        using var paint = new SKPaint
        {
            Typeface = skTypeface,
            TextSize = font.Size,
            IsAntialias = true,
            Color = new SKColor(textColor.RedComponent, textColor.GreenComponent, textColor.BlueComponent, textColor.AlphaComponent),
            Style = SKPaintStyle.Fill
        };

        var bounds = new SKRect();
        float width = paint.MeasureText(text, ref bounds);
        float height = bounds.Height;

        int imageWidth = (int)Math.Ceiling(width);
        int imageHeight = (int)Math.Ceiling(height);

        using var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.DrawText(text, -bounds.Left, -bounds.Top, paint);

        using var snapshot = surface.Snapshot();
        var pixelDataSize = imageWidth * imageHeight * 4;
        var pixelPtr = Marshal.AllocHGlobal(pixelDataSize);

        try
        {
            snapshot.ReadPixels(new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888), pixelPtr, imageWidth * 4, 0, 0);

            var pixelData = new byte[pixelDataSize];
            Marshal.Copy(pixelPtr, pixelData, 0, pixelDataSize);

            return new RgbaImage(pixelData, imageWidth, imageHeight);
        }
        finally
        {
            Marshal.FreeHGlobal(pixelPtr);
        }
    }

    /// <inheritdoc />
    public UI.Coordinates.Size ComputeTextSize(string text, Font font)
    {
        var img = ConvertTextToRgbaImage(text, font, Color.Red);
        return new UI.Coordinates.Size(img.Width, img.Height);
    }
}
