using RetroDev.OpenUI.Graphics;

namespace RetroDev.OpenUI.Themes;

/// <summary>
/// The base class for themes.
/// </summary>
public abstract class ThemeBase
{
    private Dictionary<string, Color> _colors;

    public IReadOnlyDictionary<string, Color> Colors => _colors;

    /// <summary>
    /// Creates a new theme.
    /// </summary>
    /// <param name="colors">The name - color mapping.</param>
    protected ThemeBase(Dictionary<string, Color> colors)
    {
        _colors = colors;
    }
}
