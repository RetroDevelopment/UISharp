using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoSize;

/// <summary>
/// A strategy to determine the location size of a component.
/// </summary>
public interface IAutoSizeStrategy
{
    /// <summary>
    /// Computes given <paramref name="component"/> rendering area.
    /// </summary>
    /// <param name="component">The component for which to calculate the area for.</param>
    /// <returns>Location and size of the component.</returns>
    (PixelUnit X, PixelUnit Width) ComputeHorizontalArea(UIComponent component);

    (PixelUnit Y, PixelUnit Height) ComputeVerticalArea(UIComponent component);
}
