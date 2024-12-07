namespace RetroDev.OpenUI.Graphics;

/// <summary>
/// Represents a RGBA color.
/// </summary>
/// <param name="Red">The red component value (0 darkest, 255 brightest).</param>
/// <param name="Green">The green component value (0 darkest, 255 brightest).</param>
/// <param name="Blue">The blue component value (0 darkest, 255 brightest).</param>
/// <param name="Alpha">The alpha component value (0 darkest, 255 brightest).</param>
public record Color(byte Red, byte Green, byte Blue, byte Alpha);
