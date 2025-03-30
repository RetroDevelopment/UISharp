using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Components.Core.AutoArea;

/// <summary>
/// Auto size strategy that computes components size as the maximum value between <see cref="Wrap"/> and <see cref="Stretch"/>.
/// This is useful to guarantee that the component is at least as big as its container, but even bigger if the container cannot display the full component.
/// </summary>
public class MaxWrapStretch : IAutoSize
{
    /// <summary>
    /// Computes a component width so that it has the minimal optimal size allowing to display the full component.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="wrapSize">The component wrap size.</param>
    /// <returns>The parent width.</returns>
    public PixelUnit ComputeWidth(Size parentSize, Size wrapSize) =>
        Math.Max(wrapSize.Width, parentSize.Width);

    /// <summary>
    /// Computes a component height so that it the minimal optimal size allowing to display the full component.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="wrapSize">The component wrap size.</param>
    /// <returns>The component height.</returns>
    public PixelUnit ComputeHeight(Size parentSize, Size wrapSize) =>
        Math.Max(wrapSize.Height, parentSize.Height);
}
