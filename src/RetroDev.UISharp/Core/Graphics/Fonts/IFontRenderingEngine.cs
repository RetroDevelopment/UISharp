using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics.Imaging;

namespace RetroDev.UISharp.Core.Graphics.Fonts;

/// <summary>
/// The engine rendering a ttf font into a <see cref="Image"/>.
/// </summary>
public interface IFontRenderingEngine
{
    /// <summary>
    /// Converts the given <paramref name="text"/> into a rgba image, used by <see cref="IRenderingEngine"/> to render images.
    /// </summary>
    /// <param name="text">The text to represent as <see cref="Image"/>.</param>
    /// <param name="font">The text font.</param>
    /// <param name="textColor">The text color.</param>
    /// <returns>The <see cref="Image"/> containing the text to display.</returns>
    GrayscaleImage ConvertTextToGrayscaleImage(string text, Font font, Color textColor);

    /// <summary>
    /// Computes the size required to render the given <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text to represent as <see cref="Image"/>.</param>
    /// <param name="font">The text font.
    /// <returns>The text size.</returns>
    Size ComputeTextSize(string text, Font font);

    /// <summary>
    /// Computes each character width separately for the given <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text for which to compute widths.</param>
    /// <param name="font">The font the text is rendered.</param>
    /// <returns>
    /// An array of widths <c>W</c> where <c>W[i]</c> is the total width occupide by
    /// <paramref name="text"/><c>[i]</c> including advance.
    /// </returns>
    PixelUnit[] ComputeCharactersWidths(string text, Font font);

    /// <summary>
    /// Gets the maximum height occupied by a line of text using the given <paramref name="font"/>.
    /// </summary>
    /// <param name="font">The font for which to compute the height.</param>
    /// <returns>The minimum height necessary to render any character using the given <paramref name="font"/>.</returns>
    PixelUnit ComputeTextMaximumHeight(Font font);
}
