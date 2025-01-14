using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoArea;

/// <summary>
/// Top vertical alignment.
/// </summary>
public class Top : IVerticalAlignment
{
    /// <summary>
    /// Computes the given <paramref name="component"/> topmost y-coordinate respect to its container.
    /// </summary>
    /// <param name="component">The component for which to calculate the y-coordinate.</param>
    /// <param name="componentSize">The component assumed size.</param>
    /// <returns>The component y-coordinate.</returns>
    public PixelUnit ComputeY(UIComponent component, Size componentSize) => 0;
}
