using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Core.AutoArea;

/// <summary>
/// Auto size strategy that computes components size so that they have the minimal optimal size allowing to display the full components.
/// </summary>
public class Wrap : IAutoSize
{
    /// <summary>
    /// Computes given <paramref name="component"/> width so that it has the minimal optimal size allowing to display the full component.
    /// </summary>
    /// <param name="component">The component for which to calculate the width.</param>
    /// <returns>The parent width.</returns>
    public PixelUnit ComputeWidth(UIComponent component) => component.SizeHint.Width;

    /// <summary>
    /// Computes given <paramref name="component"/> height so that it the minimal optimal size allowing to display the full component.
    /// </summary>
    /// <param name="component">The component for which to calculate the height.</param>
    /// <returns>The component height.</returns>
    public PixelUnit ComputeHeight(UIComponent component) => component.SizeHint.Height;
}
