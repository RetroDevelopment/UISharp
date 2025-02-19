using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Right horizontal alignment.
/// </summary>
public class Right : IHorizontalAlignment
{
    /// <summary>
    /// Computes a component rightmost x-coordinate respect to its container.
    /// </summary>
    /// <param name="parentSize">The compoenent parent size.</param>
    /// <param name="componentSize">The actual component size.</param>
    /// <returns>The component x-coordinate.</returns>
    public PixelUnit ComputeX(Size parentSize, Size componentSize) =>
        parentSize.Width - componentSize.Width;
}
