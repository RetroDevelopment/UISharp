using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Auto size strategy that computes components size so that they match their paraent's size.
/// </summary>
public class Stretch : IAutoSize
{
    /// <summary>
    /// Computes a component width so that it fits its parent's width.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="wrapSize">The component wrap size.</param>
    /// <returns>The parent width.</returns>
    public PixelUnit ComputeWidth(Size parentSize, Size wrapSize) =>
        parentSize.Width;

    /// <summary>
    /// Computes a component height so that it fits its parent's height.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="wrapSize">The component wrap size.</param>
    /// <returns>The component height.</returns>
    public PixelUnit ComputeHeight(Size parentSize, Size wrapSize) =>
        parentSize.Height;
}
