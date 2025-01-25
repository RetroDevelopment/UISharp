using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Generic interface to define how to align a component vertically.
/// </summary>
public interface IVerticalAlignment
{
    /// <summary>
    /// Computes the given <paramref name="component"/> y-coordinate.
    /// </summary>
    /// <param name="component">The component for which to calculate the y-coordinate.</param>
    /// <param name="componentSize">The component assumed size.</param>
    /// <returns>The component y-coordinate.</returns>
    public PixelUnit ComputeY(UIComponent component, Size componentSize);
}
