using RetroDev.OpenUI.Graphics;

namespace RetroDev.OpenUI.Themes;

/// <summary>
/// The base class for themes.
/// </summary>
public abstract class ThemeBase
{
    /// <summary>
    /// Defines a name - color mapping.
    /// </summary>
    public Dictionary<string, Color> Colors;

    /// <summary>
    /// Creates a new theme.
    /// </summary>
    /// <param name="colors">The name - color mapping.</param>
    protected ThemeBase(Dictionary<string, Color> colors)
    {
        Colors = colors;
    }
}
