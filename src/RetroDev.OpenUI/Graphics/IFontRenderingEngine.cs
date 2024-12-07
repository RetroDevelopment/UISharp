using RetroDev.OpenUI.Graphics;

namespace RetroDev.OpenUI.Core;

public interface IFontRenderingEngine
{
    /// <summary>
    /// Converts the given <paramref name="text"/> into a rgba image, used by <see cref="IRenderingEngine"/> to render images.
    /// </summary>
    /// <param name="text">The text to represent as <see cref="RgbaImage"/>.</param>
    /// <param name="fontSize">The text font size.</param>
    /// <returns></returns>
    RgbaImage ConvertTextToRgbaImage(string text, int fontSize, Color textColor);
}
