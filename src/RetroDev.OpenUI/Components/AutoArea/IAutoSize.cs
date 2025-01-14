using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoArea;

/// <summary>
/// A strategy to determine the location size of a component.
/// </summary>
public interface IAutoSize
{
    /// <summary>
    /// Computes given <paramref name="component"/> width.
    /// </summary>
    /// <param name="component">The component for which to calculate the width.</param>
    /// <returns>The component width.</returns>
    PixelUnit ComputeWidth(UIComponent component);

    /// <summary>
    /// Computes given <paramref name="component"/> height.
    /// </summary>
    /// <param name="component">The component for which to calculate the height.</param>
    /// <returns>The component height.</returns>
    PixelUnit ComputeHeight(UIComponent component);
}
