using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Internal;

namespace RetroDev.OpenUI.Graphics.Internal;

internal class FontServices : IFontServices
{
    /// <summary>
    /// Calculates the text width and height.
    /// </summary>
    /// <param name="text">The text for which to calculate the size.</param>
    /// <returns>The total text size in pixe.</returns>
    public Size ComputeTextSize(string text)
    {
        if (string.IsNullOrEmpty(text)) return Size.Zero;
        // TODO: optimize so that we don't need to calculate the size every time
        var textureImage = new SixLaborsFontRenderingEngine().ConvertTextToRgbaImage(text, 20, new Color(0, 0, 0, 0));
        return new(textureImage.Width, textureImage.Height);
    }
}
