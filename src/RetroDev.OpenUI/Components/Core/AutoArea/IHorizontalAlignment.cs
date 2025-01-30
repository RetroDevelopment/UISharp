using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Generic interface to define how to align a component horizontally.
/// </summary>
public interface IHorizontalAlignment
{
    /// <summary>
    /// Computes a compomnent x-coordinate.
    /// </summary>
    /// <param name="parentSize">The compoenent parent size.</param>
    /// <param name="componentSize">The actual component size.</param>
    /// <returns>The component x-coordinate.</returns>
    public PixelUnit ComputeX(Size parentSize, Size componentSize);
}
