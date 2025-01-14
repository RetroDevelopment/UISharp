using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoArea;

/// <summary>
/// Center (horizontal or vertical) alignment.
/// </summary>
public class Center : IAlignment
{
    /// <summary>
    /// Computes the given <paramref name="component"/> center x-coordinate respect to its container.
    /// </summary>
    /// <param name="component">The component for which to calculate the x-coordinate.</param>
    /// <param name="componentSize">The component assumed size.</param>
    /// <returns>The component x-coordinate.</returns>
    public PixelUnit ComputeX(UIComponent component, Size componentSize) =>
        ComputeCenterPoint(component, componentSize).X;

    /// <summary>
    /// Computes the given <paramref name="component"/> center y-coordinate respect to its container.
    /// </summary>
    /// <param name="component">The component for which to calculate the y-coordinate.</param>
    /// <param name="componentSize">The component assumed size.</param>
    /// <returns>The component y-coordinate.</returns>
    public PixelUnit ComputeY(UIComponent component, Size componentSize) =>
        ComputeCenterPoint(component, componentSize).Y;

    private Point ComputeCenterPoint(UIComponent component, Size size) =>
        new Area(Point.Zero, size).CenterTopLeft(new Area(Point.Zero, component.ContainerSize));
}
