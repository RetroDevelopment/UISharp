using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Center (horizontal or vertical) alignment.
/// </summary>
public class Center : IAlignment
{
    /// <summary>
    /// Computes a component center x-coordinate respect to its container.
    /// </summary>
    /// <param name="parentSize">The compoenent parent size.</param>
    /// <param name="componentSize">The actual component size.</param>
    /// <returns>The component x-coordinate.</returns>
    public PixelUnit ComputeX(Size parentSize, Size componentSize) =>
        ComputeCenterPoint(parentSize, componentSize).X;

    /// <summary>
    /// Computes a component center y-coordinate respect to its container.
    /// </summary>
    /// <param name="parentSize">The compoenent parent size.</param>
    /// <param name="componentSize">The actual component size.</param>
    /// <returns>The component y-coordinate.</returns>
    public PixelUnit ComputeY(Size parentSize, Size componentSize) =>
        ComputeCenterPoint(parentSize, componentSize).Y;

    private Point ComputeCenterPoint(Size parentSize, Size componentSize) =>
        new Area(Point.Zero, componentSize).CenterTopLeft(new Area(Point.Zero, parentSize));
}
