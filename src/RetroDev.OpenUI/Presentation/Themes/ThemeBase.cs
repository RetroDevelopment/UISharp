using System.Xml.Linq;
using RetroDev.OpenUI.Core.Graphics;

namespace RetroDev.OpenUI.Presentation.Themes;

/// <summary>
/// The base class for themes.
/// </summary>
public abstract class ThemeBase
{
    private Dictionary<string, Color> _colors = [];

    /// <summary>
    /// The name - color mapping.
    /// </summary>
    public IReadOnlyDictionary<string, Color> Colors => _colors;

    /// <summary>
    /// Creates a new theme.
    /// </summary>
    protected ThemeBase()
    {
    }

    /// <summary>
    /// Clears the current color mapping (<see cref="Colors"/>) and sets the specified <paramref name="colors"/> mapping.
    /// </summary>
    /// <param name="colors">The mapping to set.</param>
    public void SetColorDictionary(Dictionary<string, Color> colors)
    {
        _colors.Clear();
        foreach (var keyValuePair in colors)
        {
            SetColor(keyValuePair.Key, keyValuePair.Value);
        }
    }

    /// <summary>
    /// Adds or updates the color mapping identified by the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The mapping name identifier.</param>
    /// <param name="color">The color value.</param>
    public void SetColor(string name, Color color)
    {
        if (_colors.ContainsKey(name))
        {
            _colors[name] = color;
        }
        else
        {
            _colors.Add(name, color);
        }

        UpdateColorPropertyIfAny(name, color);
    }

    private void UpdateColorPropertyIfAny(string name, Color color)
    {
        var colorProperty = GetType().GetProperties().FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (colorProperty != null)
        {
            var valueProperty = colorProperty.GetValue(this);
            if (valueProperty == null) return;
            var valuePropertyType = valueProperty.GetType().GetProperties().FirstOrDefault(p => p.Name == "Value");
            if (valuePropertyType == null) return;
            valuePropertyType.SetValue(valueProperty, color);
        }
    }
}
