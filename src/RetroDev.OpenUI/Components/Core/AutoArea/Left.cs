using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Left horizontal alignment.
/// </summary>
public class Left : IHorizontalAlignment
{
    /// <summary>
    /// Computes the given <paramref name="component"/> leftmost x-coordinate respect to its container.
    /// </summary>
    /// <param name="component">The component for which to calculate the x-coordinate.</param>
    /// <param name="componentSize">The component assumed size.</param>
    /// <returns>The component x-coordinate.</returns>
    public PixelUnit ComputeX(UIComponent component, Size componentSize) => PixelUnit.Zero;
}
