using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenUI.Core.Graphics.Fonts;

/// <summary>
/// The engine rendering a ttf font into a <see cref="RgbaImage"/>.
/// </summary>
public interface IFontRenderingEngine
{
    /// <summary>
    /// Converts the given <paramref name="text"/> into a rgba image, used by <see cref="IRenderingEngine"/> to render images.
    /// </summary>
    /// <param name="text">The text to represent as <see cref="RgbaImage"/>.</param>
    /// <param name="font">The text font.</param>
    /// <param name="textColor">The text color.</param>
    /// <returns>The <see cref="RgbaImage"/> containing the text to display.</returns>
    RgbaImage ConvertTextToRgbaImage(string text, Font font, Color textColor);

    /// <summary>
    /// Computes the size required to render the given <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text to represent as <see cref="RgbaImage"/>.</param>
    /// <param name="font">The text font.
    /// <returns>The text size.</returns>
    Size ComputeTextSize(string text, Font font);

    /// <summary>
    /// Gets the maximum height occupied by a line of text using the given <paramref name="font"/>.
    /// </summary>
    /// <param name="font">The font for which to compute the height.</param>
    /// <returns>The minimum height necessary to render any character using the given <paramref name="font"/>.</returns>
    PixelUnit ComputeTextMaximumHeight(Font font);
}
