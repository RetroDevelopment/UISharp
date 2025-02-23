using FreeTypeSharp;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Graphics.Imaging;
using System.Globalization;
using static FreeTypeSharp.FT;

namespace RetroDev.OpenUI.Core.Graphics.Fonts;

/// <summary>
/// Font rendering engine using FreeTypeSharp library.
/// This allows for a fine graained control over font rendering and positioning and it is efficient as
/// it uses a direct native FreeType library implementation.
/// </summary>
public class FreeTypeFontRenderingEngine : IFontRenderingEngine
{
    private record struct Gliph(GrayscaleImage Image, int Advance, int BearingX, int BearingY);

    private readonly unsafe FT_LibraryRec_* _library = null;
    private readonly unsafe Dictionary<Font, IntPtr> _fontCache = [];
    private readonly Dictionary<Font, Dictionary<uint, Gliph>> _gliphCache = [];

    /// <summary>
    /// Creates a new rendering engine.
    /// </summary>
    public FreeTypeFontRenderingEngine()
    {
        unsafe
        {
            // TODO: implement destructors
            FT_LibraryRec_* library;
            var error = FT_Init_FreeType(&library);
            ThrowIfError(error, "loading font library");
            _library = library;
        }
    }

    /// <inheritdoc />
    public Size ComputeTextSize(string text, Font font)
    {
        if (string.IsNullOrEmpty(text)) return Size.Zero;
        EnsureFontIsLoaded(font);
        var metrics = CalculateFontMetrics(font);
        var gliphs = GetCharacters(text, font);
        var height = metrics.height + 1;
        var width = ComputeTextMaximumWidth(gliphs).Width;
        return new Size(width, height);
    }

    /// <inheritdoc />
    public GrayscaleImage ConvertTextToGrayscaleImage(string text, Font font, Color textColor)
    {
        if (string.IsNullOrEmpty(text)) return GrayscaleImage.Empty;

        EnsureFontIsLoaded(font);
        var metrics = CalculateFontMetrics(font);
        var gliphs = GetCharacters(text, font);
        var height = metrics.height + 1;
        var info = ComputeTextMaximumWidth(gliphs);
        var width = info.Width;
        var image = CreateTextImageFromGliphs(gliphs, width, height, metrics.ascender, info.XOffset);
        return image;
    }

    /// <inheritdoc />
    public PixelUnit ComputeTextMaximumHeight(Font font)
    {
        EnsureFontIsLoaded(font);
        var metrics = CalculateFontMetrics(font);
        return metrics.height + 1;
    }

    private (int Width, int XOffset) ComputeTextMaximumWidth(IEnumerable<Gliph> gliphs)
    {
        if (!gliphs.Any()) return (0, 0);

        var width = 0;
        var xOffset = 0;
        var firstGliph = gliphs.First();
        var lastGliph = gliphs.Last();

        width = gliphs.Sum((g) => g.Advance);
        if (firstGliph.BearingX < 0)
        {
            xOffset = -firstGliph.BearingX;
            width += xOffset;
        }

        if (lastGliph.Image.Size.Width > lastGliph.Advance)
        {
            width -= lastGliph.Advance;
            width += (int)lastGliph.Image.Size.Width;
        }

        return (width + 1, xOffset);
    }

    private GrayscaleImage CreateTextImageFromGliphs(IEnumerable<Gliph> gliphs, int width, int height, int ascender, int xOffset)
    {
        var image = new GrayscaleImage(new Size(width, height));
        var xPosition = 0;
        var baseline = ascender;

        foreach (var gliph in gliphs)
        {
            var x = xPosition + gliph.BearingX;
            var y = baseline - gliph.BearingY;
            var offset = new Point(x + xOffset, y);
            image.CopyFrom(gliph.Image, offset);
            xPosition += gliph.Advance;
        }

        return image;
    }

    private void EnsureFontIsLoaded(Font font)
    {
        unsafe
        {
            if (!_fontCache.ContainsKey(font))
            {
                fixed (byte* fontDataPtr = font.Data)
                {
                    FT_FaceRec_* newFace;
                    var error = FT_New_Memory_Face(_library, fontDataPtr, font.Data.Length, 0, &newFace);
                    ThrowIfError(error, $"loading font {font}");
                    _fontCache.Add(font, (IntPtr)newFace);
                }
            }
        }
    }

    private unsafe (int ascender, int descender, int height) CalculateFontMetrics(Font font)
    {
        unsafe
        {
            var face = (FT_FaceRec_*)_fontCache[font];
            var size = font.Size;
            var error = FT_Set_Pixel_Sizes(face, 0, (uint)size);
            ThrowIfError(error, "setting char size");
            var ascender = (int)face->size->metrics.ascender >> 6;  // Max height above baseline. Convert from 26.6 fixed point
            var descender = (int)face->size->metrics.descender >> 6; // Max height below baseline. Convert from 26.6 fixed point
            var height = (int)face->size->metrics.height >> 6;    // Height. Recommended line height
            return (ascender, descender, height);
        }
    }

    private IEnumerable<Gliph> GetCharacters(string text, Font font)
    {
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
        var gliphs = new List<Gliph>();

        unsafe
        {
            var face = (FT_FaceRec_*)_fontCache[font];
            while (enumerator.MoveNext())
            {
                var grapheme = enumerator.GetTextElement(); // Get full Unicode character
                var codepoint = char.ConvertToUtf32(grapheme, 0); // Convert to UTF-32
                var gliph = LoadCharacter(font, (uint)codepoint);
                gliphs.Add(gliph);
            }
        }

        return gliphs;
    }

    private Gliph LoadCharacter(Font font, uint code)
    {
        unsafe
        {
            var face = (FT_FaceRec_*)_fontCache[font];
            _gliphCache.TryAdd(font, []);
            var gliphDictionary = _gliphCache[font];
            if (gliphDictionary.ContainsKey(code)) return gliphDictionary[code];

            var error = FT_Load_Char(face, code, FT_LOAD.FT_LOAD_RENDER); // Call FreeType
            ThrowIfError(error, $"loading character with unicode code {code}");
            var glyph = face->glyph;
            var bitmap = glyph->bitmap;
            var width = bitmap.width;
            var height = bitmap.rows;
            var pitch = bitmap.pitch; // Number of bytes per row
            GrayscaleImage image;
            if (bitmap.buffer == null || width == 0 || height == 0)
            {
                image = GrayscaleImage.Empty;
            }
            else
            {
                var grayscaleBitmap = new byte[width * height];
                // Copy grayscale bitmap from FreeType buffer
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        grayscaleBitmap[y * width + x] = bitmap.buffer[y * pitch + x];
                    }
                }

                image = new GrayscaleImage(grayscaleBitmap, new Size(width, height));
            }

            var advance = glyph->advance.x >> 6;  // Convert from 26.6 fixed point to integer pixels
            var bearingX = glyph->bitmap_left;
            var bearingY = glyph->bitmap_top;
            var cacheGliph = new Gliph(image, (int)advance, bearingX, bearingY);
            gliphDictionary.Add(code, cacheGliph);
            return cacheGliph;
        }
    }

    private void ThrowIfError(FT_Error error, string operation)
    {
        if (error != FT_Error.FT_Err_Ok)
        {
            throw new InvalidOperationException($"Font error while {operation}: {error}");
        }
    }
}
