using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Components.Core.AutoArea;

/// <summary>
/// Bottom vertical alignment.
/// </summary>
public class Bottom : IVerticalAlignment
{
    /// <summary>
    /// Computes a component bottommost y-coordinate respect to its container.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="componentSize">The actual component size.</param>
    /// <returns>The component y-coordinate.</returns>
    public PixelUnit ComputeY(Size parentSize, Size componentSize) =>
        parentSize.Height - componentSize.Height;
}
