using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Auto size strategy that computes components size so that they have the minimal optimal size allowing to display the full components.
/// </summary>
public class Wrap : IAutoSize
{
    /// <summary>
    /// Computes a component width so that it has the minimal optimal size allowing to display the full component.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="wrapSize">The component wrap size.</param>
    /// <returns>The parent width.</returns>
    public PixelUnit ComputeWidth(Size parentSize, Size wrapSize) => wrapSize.Width;

    /// <summary>
    /// Computes a component height so that it the minimal optimal size allowing to display the full component.
    /// </summary>
    /// <param name="parentSize">The parent component size.</param>
    /// <param name="wrapSize">The component wrap size.</param>
    /// <returns>The component height.</returns>
    public PixelUnit ComputeHeight(Size parentSize, Size wrapSize) => wrapSize.Height;
}
