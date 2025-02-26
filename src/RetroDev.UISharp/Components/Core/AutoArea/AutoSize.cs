namespace RetroDev.UISharp.Components.Core.AutoArea;

/// <summary>
/// Contains all the pre-defined auto <see cref="IAutoSize"/>.
/// </summary>
public static class AutoSize
{
    /// <summary>
    /// Stretch the component to match the parent size.
    /// </summary>
    public static readonly IAutoSize Stretch = new Stretch();

    /// <summary>
    /// Use the component using the optimal size to display the full component.
    /// </summary>
    public static readonly IAutoSize Wrap = new Wrap();
}
