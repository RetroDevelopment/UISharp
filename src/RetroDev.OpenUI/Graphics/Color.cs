using System.Text.RegularExpressions;

namespace RetroDev.OpenUI.Graphics;

/// <summary>
/// Represents a RGBA color.
/// </summary>
/// <param name="RedComponent">The red component value (0 darkest, 255 brightest).</param>
/// <param name="GreenComponent">The green component value (0 darkest, 255 brightest).</param>
/// <param name="BlueComponent">The blue component value (0 darkest, 255 brightest).</param>
/// <param name="AlphaComponent">The alpha component value (0 darkest, 255 brightest).</param>
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
    public static readonly Color Green = new Color(0, 255, 0, 255);

    /// <summary>
    /// Fully opaque blue color.
    /// </summary>
    public static readonly Color Blue = new Color(0, 0, 255, 255);

    /// <summary>
    /// Fully opaque cyan color (aqua).
    /// </summary>
    public static readonly Color Cyan = new Color(0, 255, 255, 255);

    /// <summary>
    /// Fully opaque magenta color.
    /// </summary>
    public static readonly Color Magenta = new Color(255, 0, 255, 255);

    /// <summary>
    /// Fully opaque yellow color.
    /// </summary>
    public static readonly Color Yellow = new Color(255, 255, 0, 255);

    /// <summary>
    /// Fully opaque light gray color.
    /// </summary>
    public static readonly Color LightGray = new Color(211, 211, 211, 255);

    /// <summary>
    /// Fully opaque gray color.
    /// </summary>
    public static readonly Color Gray = new Color(128, 128, 128, 255);

    /// <summary>
    /// Fully opaque dark gray color.
    /// </summary>
    public static readonly Color DarkGray = new Color(64, 64, 64, 255);

    /// <summary>
    /// Fully opaque light red color.
    /// </summary>
    public static readonly Color LightRed = new Color(255, 102, 102, 255);

    /// <summary>
    /// Fully opaque dark red color.
    /// </summary>
    public static readonly Color DarkRed = new Color(139, 0, 0, 255);

    /// <summary>
    /// Fully opaque light green color.
    /// </summary>
    public static readonly Color LightGreen = new Color(144, 238, 144, 255);

    /// <summary>
    /// Fully opaque dark green color.
    /// </summary>
    public static readonly Color DarkGreen = new Color(0, 100, 0, 255);

    /// <summary>
    /// Fully opaque light blue color.
    /// </summary>
    public static readonly Color LightBlue = new Color(173, 216, 230, 255);

    /// <summary>
    /// Fully opaque dark blue color.
    /// </summary>
    public static readonly Color DarkBlue = new Color(0, 0, 139, 255);

    /// <summary>
    /// Fully opaque orange color.
    /// </summary>
    public static readonly Color Orange = new Color(255, 165, 0, 255);

    /// <summary>
    /// Fully opaque purple color.
    /// </summary>
    public static readonly Color Purple = new Color(128, 0, 128, 255);

    /// <summary>
    /// Fully opaque brown color.
    /// </summary>
    public static readonly Color Brown = new Color(165, 42, 42, 255);

    /// <summary>
    /// Fully opaque gold color.
    /// </summary>
    public static readonly Color Gold = new Color(255, 215, 0, 255);

    /// <summary>
    /// Fully opaque pink color.
    /// </summary>
    public static readonly Color Pink = new Color(255, 192, 203, 255);

    /// <summary>
    /// Fully opaque navy color.
    /// </summary>
    public static readonly Color Navy = new Color(0, 0, 128, 255);

    /// <summary>
    /// Fully opaque teal color.
    /// </summary>
    public static readonly Color Teal = new Color(0, 128, 128, 255);

    /// <summary>
    /// Fully opaque olive color.
    /// </summary>
    public static readonly Color Olive = new Color(128, 128, 0, 255);

    /// <summary>
    /// Fully opaque beige color.
    /// </summary>
    public static readonly Color Beige = new Color(245, 245, 220, 255);

    /// <summary>
    /// Fully opaque mint color.
    /// </summary>
    public static readonly Color Mint = new Color(189, 252, 201, 255);

    /// <summary>
    /// Whether <see langword="this" /> color is <see cref="Transparent"/>.
    /// </summary>
    public bool IsTransparent => this == Transparent;

    /// <summary>
    /// Creates a new color from the given <paramref name="colorHex"/> representation.
    /// </summary>
    /// <param name="colorHex">
    /// The color hexadecimal represnetation in the form #RRGGBBAA (case insensitive).
    /// For example #1A2B3C4D and #1a2b3c4d are valid color strings.
    /// </param>
    /// <returns>The <see cref="Color"/> representing the given hexadecimal string <paramref name="colorHex"/>.</returns>
    /// <exception cref="ArgumentException">If the color hexadecimal string is not in the correct format.</exception>
    public static Color FromHex(string colorHex)
    {
        var regex = new Regex(@"^#(?<red>[0-9A-Fa-f]{2})(?<green>[0-9A-Fa-f]{2})(?<blue>[0-9A-Fa-f]{2})(?<alpha>[0-9A-Fa-f]{2})$");
        var match = regex.Match(colorHex);

        if (match.Success)
        {
            var red = Convert.ToByte(match.Groups["red"].Value, 16);
            var green = Convert.ToByte(match.Groups["green"].Value, 16);
            var blue = Convert.ToByte(match.Groups["blue"].Value, 16);
            var alpha = Convert.ToByte(match.Groups["alpha"].Value, 16);

            return new Color(red, green, blue, alpha);
        }
        else
        {
            throw new ArgumentException($"Invalid color hex string {colorHex}. The string must be in the #RRGGBBAA format.");
        }

    }
}
