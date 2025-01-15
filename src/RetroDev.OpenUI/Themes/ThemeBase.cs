using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Themes;

/// <summary>
/// The base class for themes.
/// </summary>
public abstract class ThemeBase
{
    private Dictionary<string, Color> _colors = [];

    public IReadOnlyDictionary<string, Color> Colors => _colors;

    /// <summary>
    /// Creates a new theme.
    /// </summary>
    protected ThemeBase()
    {
    }

    public void SetColorDictionary(Dictionary<string, Color> colors)
    {
        _colors.Clear();
        foreach (var keyValuePair in colors)
        {
            SetColor(keyValuePair.Key, keyValuePair.Value);
        }
    }

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
