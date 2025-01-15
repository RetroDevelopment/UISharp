using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Themes;

/// <summary>
/// The base class represnting a OpenUI theme. It contains all fields needed by OpenUI default components.
/// Extend this class to include additional theme parameters.
/// </summary>
public class Theme : ThemeBase
{
    public BindableProperty<Theme, Color> PrimaryColor { get; }

    /// <summary>
    /// Creates a new theme.
    /// </summary>
    /// <param name="colors">The name - color mapping.</param>
    public Theme(Application application) : base()
    {
        PrimaryColor = new BindableProperty<Theme, Color>(this, Color.Transparent, application);
    }
}
