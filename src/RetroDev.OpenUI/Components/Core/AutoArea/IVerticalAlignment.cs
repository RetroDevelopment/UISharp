using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Generic interface to define how to align a component vertically.
/// </summary>
public interface IVerticalAlignment
{
    /// <summary>
    /// Computes a compomnent y-coordinate.
    /// </summary>
    /// <param name="parentSize">The compoenent parent size.</param>
    /// <param name="componentSize">The actual component size.</param>
    /// <returns>The component y-coordinate.</returns>
    public PixelUnit ComputeY(Size parentSize, Size componentSize);
}
