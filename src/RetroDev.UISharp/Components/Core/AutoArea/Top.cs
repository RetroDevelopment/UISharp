using RetroDev.UISharp.Core.Graphics.Coordinates;

namespace RetroDev.UISharp.Components.Core.AutoArea;

/// <summary>
/// Top vertical alignment.
/// </summary>
public class Top : IVerticalAlignment
{
    /// <summary>
    /// Computes a component topmost y-coordinate respect to its container.
    /// </summary>
    /// <param name="parentSize">The compoenent parent size.</param>
    /// <param name="componentSize">The actual component size.</param>
    /// <returns>The component y-coordinate.</returns>
    public PixelUnit ComputeY(Size parentSize, Size componentSize) => 0;
}
