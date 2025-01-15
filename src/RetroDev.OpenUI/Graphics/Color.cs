using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RetroDev.OpenUI.Graphics;

/// <summary>
/// Represents a RGBA color.
/// </summary>
/// <param name="RedComponent">The red component value (0 darkest, 255 brightest).</param>
/// <param name="GreenComponent">The green component value (0 darkest, 255 brightest).</param>
/// <param name="BlueComponent">The blue component value (0 darkest, 255 brightest).</param>
/// <param name="AlphaComponent">The alpha component value (0 darkest, 255 brightest).</param>
[DebuggerDisplay("({RedComponent}, {GreenComponent}, {BlueComponent}, {AlphaComponent}))")]
public readonly record struct Color(byte RedComponent = 0, byte GreenComponent = 0, byte BlueComponent = 0, byte AlphaComponent = 0)
{
    /// <summary>
    /// The transparent color.
    /// </summary>
    public static readonly Color Transparent = default;

    /// <summary>
    /// Fully opaque white color.
    /// </summary>
    public static readonly Color White = new(255, 255, 255, 255);

    /// <summary>
    /// Fully opaque black color.
    /// </summary>
    public static readonly Color Black = new(0, 0, 0, 255);

    /// <summary>
    /// Fully opaque red color.
    /// </summary>
    public static readonly Color Red = new(255, 0, 0, 255);

    /// <summary>
    /// Fully opaque green color.
    /// </summary>
    public static readonly Color Green = new(0, 255, 0, 255);

    /// <summary>
    /// Fully opaque blue color.
    /// </summary>
    public static readonly Color Blue = new(0, 0, 255, 255);

    /// <summary>
    /// Fully opaque cyan color (aqua).
    /// </summary>
    public static readonly Color Cyan = new(0, 255, 255, 255);

    /// <summary>
    /// Fully opaque magenta color.
    /// </summary>
    public static readonly Color Magenta = new(255, 0, 255, 255);

    /// <summary>
    /// Fully opaque yellow color.
    /// </summary>
    public static readonly Color Yellow = new(255, 255, 0, 255);

    /// <summary>
    /// Fully opaque light gray color.
    /// </summary>
    public static readonly Color LightGray = new(211, 211, 211, 255);

    /// <summary>
    /// Fully opaque gray color.
    /// </summary>
    public static readonly Color Gray = new(128, 128, 128, 255);

    /// <summary>
    /// Fully opaque dark gray color.
    /// </summary>
    public static readonly Color DarkGray = new(64, 64, 64, 255);

    /// <summary>
    /// Fully opaque light red color.
    /// </summary>
    public static readonly Color LightRed = new(255, 102, 102, 255);

    /// <summary>
    /// Fully opaque dark red color.
    /// </summary>
    public static readonly Color DarkRed = new(139, 0, 0, 255);

    /// <summary>
    /// Fully opaque light green color.
    /// </summary>
    public static readonly Color LightGreen = new(144, 238, 144, 255);

    /// <summary>
    /// Fully opaque dark green color.
    /// </summary>
    public static readonly Color DarkGreen = new(0, 100, 0, 255);

    /// <summary>
    /// Fully opaque light blue color.
    /// </summary>
    public static readonly Color LightBlue = new(173, 216, 230, 255);

    /// <summary>
    /// Fully opaque dark blue color.
    /// </summary>
    public static readonly Color DarkBlue = new(0, 0, 139, 255);

    /// <summary>
    /// Fully opaque orange color.
    /// </summary>
    public static readonly Color Orange = new(255, 165, 0, 255);

    /// <summary>
    /// Fully opaque purple color.
    /// </summary>
    public static readonly Color Purple = new(128, 0, 128, 255);

    /// <summary>
    /// Fully opaque brown color.
    /// </summary>
    public static readonly Color Brown = new(165, 42, 42, 255);

    /// <summary>
    /// Fully opaque gold color.
    /// </summary>
    public static readonly Color Gold = new(255, 215, 0, 255);

    /// <summary>
    /// Fully opaque pink color.
    /// </summary>
    public static readonly Color Pink = new(255, 192, 203, 255);

    /// <summary>
    /// Fully opaque navy color.
    /// </summary>
    public static readonly Color Navy = new(0, 0, 128, 255);

    /// <summary>
    /// Fully opaque teal color.
    /// </summary>
    public static readonly Color Teal = new(0, 128, 128, 255);

    /// <summary>
    /// Fully opaque olive color.
    /// </summary>
    public static readonly Color Olive = new(128, 128, 0, 255);

    /// <summary>
    /// Fully opaque beige color.
    /// </summary>
    public static readonly Color Beige = new(245, 245, 220, 255);

    /// <summary>
    /// Fully opaque mint color.
    /// </summary>
    public static readonly Color Mint = new(189, 252, 201, 255);

    /// <summary>
    /// Whether <see langword="this" /> color is <see cref="Transparent"/>.
    /// </summary>
    public bool IsTransparent => AlphaComponent == 0;

    /// <summary>
    /// Creates a new color from the given <paramref name="expression"/> representation.
    /// </summary>
    /// <param name="expression">
    /// The color hexadecimal represnetation in the form #RRGGBBAA (case insensitive).
    /// For example #1A2B3C4D and #1a2b3c4d are valid color strings.
    /// It can also be a color constant from the constants in the <see cref="Color"/> class (e.g. <see cref="Color.Red"/>).
    /// </param>
    /// <returns>The <see cref="Color"/> representing the given hexadecimal string <paramref name="colorHex"/>.</returns>
    /// <exception cref="ArgumentException">If the color hexadecimal string is not in the correct format.</exception>
    public Color(string expression) : this(0, 0, 0, 0)
    {
        var colorConstant = FindConstant(expression);
        if (colorConstant != null)
        {
            RedComponent = colorConstant.Value.RedComponent;
            GreenComponent = colorConstant.Value.GreenComponent;
            BlueComponent = colorConstant.Value.BlueComponent;
            AlphaComponent = colorConstant.Value.AlphaComponent;
            return;
        }

        var regex = new Regex(@"^#(?<red>[0-9A-Fa-f]{2})(?<green>[0-9A-Fa-f]{2})(?<blue>[0-9A-Fa-f]{2})(?<alpha>[0-9A-Fa-f]{2})$");
        var match = regex.Match(expression);

        if (match.Success)
        {
            RedComponent = Convert.ToByte(match.Groups["red"].Value, 16);
            GreenComponent = Convert.ToByte(match.Groups["green"].Value, 16);
            BlueComponent = Convert.ToByte(match.Groups["blue"].Value, 16);
            AlphaComponent = Convert.ToByte(match.Groups["alpha"].Value, 16);
        }
        else
        {
            throw new ArgumentException($"Invalid color hex string {expression}. The string must be in the #RRGGBBAA format.");
        }

    }

    private static Color? FindConstant(string name)
    {
        var propertyInfo = typeof(Color).GetFields(BindingFlags.Static | BindingFlags.Public)
                                        .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo == null) return null;
        var value = propertyInfo.GetValue(null);
        if (value == null) return null;
        return (Color)value;
    }
}
