using FreeTypeSharp;
using RetroDev.OpenUI.UI.Coordinates;
using System.Globalization;
using System.Runtime.InteropServices;
using static FreeTypeSharp.FT;

namespace RetroDev.OpenUI.Core.Graphics.Fonts;

public class FreeTypeFontRenderingEngine : IFontRenderingEngine
{
    private record struct GrayScaleImage(byte[] Data, int Width, int Height);
    private record struct Gliph(GrayScaleImage Image, int Advance, int BearingX, int BearingY);

    private readonly unsafe FT_LibraryRec_* _library = null;
    private readonly unsafe Dictionary<Font, IntPtr> _fontCache = [];
    private readonly Dictionary<Font, Dictionary<uint, Gliph>> _gliphCache = [];
    public FreeTypeFontRenderingEngine()
    {
        unsafe
        {
            FT_LibraryRec_* library;
            var error = FT_Init_FreeType(&library);
            ThrowIfError(error, "loading font library");
            _library = library;
        }
    }

    public Size ComputeTextSize(string text, Font font)
    {
        var img = ConvertTextToRgbaImage(text, font, Color.Transparent);
        return new Size(img.Width, img.Height);
    }

    public RgbaImage ConvertTextToRgbaImage(string text, Font font, Color textColor)
    {
        if (string.IsNullOrEmpty(text)) return RgbaImage.Empty;

        EnsureFontIsLoaded(font);
        var metrics = CalculateFontMetrics(font);
        var list = GetCharacters(text, font);
        var height = metrics.height + 1;
        var width = list.Sum(g => g.Advance) + 1;
        var image = CreateRgbaImageFromGliphs(list, width, height, metrics.ascender, textColor);
        return new RgbaImage(image, width, height);
    }

    private byte[] CreateRgbaImageFromGliphs(IEnumerable<Gliph> gliphs, int width, int height, int ascender, Color textColor)
    {
        var rgbaImageBuffer = new byte[width * height * 4];
        var xPosition = 0;
        var baseline = ascender;

        foreach (var gliph in gliphs)
        {
            var x = xPosition + gliph.BearingX;
            var y = baseline - gliph.BearingY;
            RenderGliph(ref rgbaImageBuffer, gliph.Image.Data, x, y, gliph.Image.Width, gliph.Image.Height, width, textColor);
            xPosition += gliph.Advance;
        }

        return rgbaImageBuffer;
    }

    private void RenderGliph(ref byte[] rgbaImage, byte[] grayScaleImage, int left, int top, int width, int height, int fullWidth, Color textColor)
    {
        var baseAlpha = textColor.AlphaComponent / 255.0f;

        for (var y = top; y < top + height; y++)
        {
            for (var x = left; x < left + width; x++)
            {
                var index = ((y * fullWidth) + x) * 4;
                var greyScaleX = x - left;
                var greyScaleY = y - top;
                var greyScaleArrayIndex = (greyScaleY * width) + greyScaleX;
                var fontAlfa = grayScaleImage[greyScaleArrayIndex] / 255.0f;
                rgbaImage[index] = textColor.RedComponent;
                rgbaImage[index + 1] = textColor.GreenComponent;
                rgbaImage[index + 2] = textColor.BlueComponent;
                rgbaImage[index + 3] = (byte)(baseAlpha * fontAlfa * 255.0f);
            }
        }
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
            GrayScaleImage image;
            if (bitmap.buffer == null || width == 0 || height == 0)
            {
                image = new GrayScaleImage([], 0, 0);
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

                image = new GrayScaleImage(grayscaleBitmap, (int)width, (int)height);
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
