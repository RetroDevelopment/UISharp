using RetroDev.OpenUI.Graphics;

namespace RetroDev.OpenUI.Themes;

/// <summary>
/// The base class represnting a OpenUI theme. It contains all fields needed by OpenUI default components.
/// Extend this class to include additional theme parameters.
/// </summary>
public class Theme : ThemeBase
{
    /// <summary>
    /// Creates a new theme.
    /// </summary>
    /// <param name="colors">The name - color mapping.</param>
    public Theme(Dictionary<string, Color> colors) : base(colors) { }

    public Color PrimaryColor => GetColorOrDefault("primaryColor", Color.Red);

    protected Color GetColorOrDefault(string name, Color defaultColor) =>
         Colors.ContainsKey(name) ? Colors[name] : defaultColor;
}
