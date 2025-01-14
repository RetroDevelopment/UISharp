using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoArea;

/// <summary>
/// Auto size strategy that computes components size so that they match their paraent's size.
/// </summary>
public class Stretch : IAutoSize
{
    /// <summary>
    /// Computes given <paramref name="component"/> width so that it fits its parent's width.
    /// </summary>
    /// <param name="component">The component for which to calculate the width.</param>
    /// <returns>The parent width.</returns>
    public PixelUnit ComputeWidth(UIComponent component) =>
        component.ContainerSize.Width;

    /// <summary>
    /// Computes given <paramref name="component"/> height so that it fits its parent's height.
    /// </summary>
    /// <param name="component">The component for which to calculate the height.</param>
    /// <returns>The component height.</returns>
    public PixelUnit ComputeHeight(UIComponent component) =>
        component.ContainerSize.Height;
}
