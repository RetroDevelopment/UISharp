using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Generic interface to define how to align a component horizontally.
/// </summary>
public interface IHorizontalAlignment
{
    /// <summary>
    /// Computes the given <paramref name="component"/> x-coordinate.
    /// </summary>
    /// <param name="component">The component for which to calculate the x-coordinate.</param>
    /// <param name="componentSize">The component assumed size.</param>
    /// <returns>The component x-coordinate.</returns>
    public PixelUnit ComputeX(UIComponent component, Size componentSize);
}
