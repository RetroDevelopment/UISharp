using SixLabors.ImageSharp.Formats.Png;

namespace RetroDev.OpenUI.Graphics;

/// <summary>
/// Represents a RGBA color.
/// </summary>
/// <param name="Red">The red component value (0 darkest, 255 brightest).</param>
/// <param name="Green">The green component value (0 darkest, 255 brightest).</param>
/// <param name="Blue">The blue component value (0 darkest, 255 brightest).</param>
/// <param name="Alpha">The alpha component value (0 darkest, 255 brightest).</param>
public readonly record struct Color(byte Red = 0, byte Green = 0, byte Blue = 0, byte Alpha = 0)
{
    /// <summary>
    /// The transparent color.
    /// </summary>
    public static readonly Color Transparent = default;

    /// <summary>
    /// Whether <see langword="this" /> color is <see cref="Transparent"/>.
    /// </summary>
    public bool IsTransparent => this == Transparent;
}
