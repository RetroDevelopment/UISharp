using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Right horizontal alignment.
/// </summary>
public class Right : IHorizontalAlignment
{
    /// <summary>
    /// Computes the given <paramref name="component"/> rightmost x-coordinate respect to its container.
    /// </summary>
    /// <param name="component">The component for which to calculate the x-coordinate.</param>
    /// <param name="componentSize">The component assumed size.</param>
    /// <returns>The component x-coordinate.</returns>
    public PixelUnit ComputeX(UIComponent component, Size componentSize) =>
        component.ContainerSize.Width - componentSize.Width;
}
