using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Left horizontal alignment.
/// </summary>
public class Left : IHorizontalAlignment
{
    /// <summary>
    /// Computes a component leftmost x-coordinate respect to its container.
    /// </summary>
    /// <param name="parentSize">The compoenent parent size.</param>
    /// <param name="componentSize">The actual component size.</param>
    /// <returns>The component x-coordinate.</returns>
    public PixelUnit ComputeX(Size parentSize, Size componentSize) => PixelUnit.Zero;
}
