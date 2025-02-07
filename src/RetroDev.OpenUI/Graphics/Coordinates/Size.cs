using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Coordinates;

/// <summary>
/// Represents a 2D area size.
/// </summary>
/// <param name="Width">The width in pixels.</param>
/// <param name="Height">The height in pixels.</param>
[DebuggerDisplay("{Width} x {Height}")]
public record Size(PixelUnit Width, PixelUnit Height)
{
    public static readonly Size Zero = new(0, 0);

    /// <summary>
    /// Creates a <see cref="Area"/> from (0, 0) with <see langword="this" /> size.
    /// </summary>
    /// <remarks>
    /// This method is very useful when creating a drawing area relative to a container drawing area that fills it completely.
    /// For example, given a size <c>s</c> 300 x 300, <c>s.Fill()</c> will return
    /// the area with location (0, 0) and size 300 x 300.
    /// </remarks>
    /// <returns>The area with <see cref="Point.Zero"/> coordinate and the same <see cref="Size"/> as <see langword="this" /> area.</returns>
    public Area Fill() =>
        new Area(Point.Zero, this);

    public static Size operator +(Size s1, Size s2) => new(s1.Width + s2.Width, s1.Height + s2.Height);
    public static Size operator -(Size s1, Size s2) => new(s1.Width - s2.Width, s1.Height - s2.Height);

    /// <inheritdoc />
    public override string ToString() =>
        $"{Width.Value} x {Height.Value}";
}
