using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// A strategy to determine the location size of a component.
/// </summary>
public interface IAutoSize
{
    /// <summary>
    /// Computes a component width given the <paramref name="parentSize"/>.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="wrapSize">The component wrap size.</param>
    /// <returns>The component width.</returns>
    PixelUnit ComputeWidth(Size parentSize, Size wrapSize);

    /// <summary>
    /// Computes a component height given the <paramref name="parentSize"/>.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="wrapSize">The component wrap size.</param>
    /// <returns>The component height.</returns>
    PixelUnit ComputeHeight(Size parentSize, Size wrapSize);
}
