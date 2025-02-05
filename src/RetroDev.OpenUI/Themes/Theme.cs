using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Themes;

/// <summary>
/// The base class represnting a OpenUI theme. It contains all fields needed by OpenUI default components.
/// Extend this class to include additional theme parameters.
/// </summary>
public class Theme : ThemeBase
{
    /// <summary>
    /// The default color in case a color is not defined.
    /// </summary>
    public static readonly Color DefaultColor = Color.Black;

    /// <summary>
    /// Main window background.
    /// </summary>
    public BindableProperty<Color> MainBackground { get; }

    /// <summary>
    /// Background used for containers.
    /// </summary>
    public BindableProperty<Color> PrimaryBackground { get; }

    /// <summary>
    /// Primary background color.
    /// </summary>
    public BindableProperty<Color> PrimaryColor { get; }

    /// <summary>
    /// Light version of <see cref="PrimaryColor"/> mostly used for disable components.
    /// </summary>
    public BindableProperty<Color> PrimaryColorDisabled { get; }

    /// <summary>
    /// Light version of <see cref="PrimaryColor"/> with contrasts primary color.
    /// </summary>
    public BindableProperty<Color> PrimaryColorContrast { get; }

    /// <summary>
    /// Secondary background color.
    /// </summary>
    public BindableProperty<Color> SecondaryColor { get; }

    /// <summary>
    /// Light version of <see cref="SecondaryColor"/>.
    /// </summary>
    public BindableProperty<Color> SecondaryColorDisabled { get; }

    /// <summary>
    /// The main color for text.
    /// </summary>
    public BindableProperty<Color> TextColor { get; }

    /// <summary>
    /// Light version of <see cref="TextColor"/>, usually for disabled text.
    /// </summary>
    public BindableProperty<Color> TextColorDisabled { get; }

    /// <summary>
    /// Color for border surrounding UI elements.
    /// </summary>
    public BindableProperty<Color> BorderColor { get; }

    /// <summary>
    /// Creates a new theme.
    /// </summary>
    /// <param name="colors">The name - color mapping.</param>
    public Theme(Application application) : base()
    {
        BindingType allowedBinding = BindingType.SourceToDestination;

        MainBackground = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        PrimaryBackground = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        PrimaryColor = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        PrimaryColorDisabled = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        PrimaryColorContrast = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        SecondaryColor = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        SecondaryColorDisabled = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        TextColor = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        TextColorDisabled = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
        BorderColor = new BindableProperty<Color>(DefaultColor, application, allowedBinding);
    }
}
