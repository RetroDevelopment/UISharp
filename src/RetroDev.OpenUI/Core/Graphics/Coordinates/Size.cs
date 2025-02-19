using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Graphics.Coordinates;

/// <summary>
/// Represents a 2D area size.
/// </summary>
[DebuggerDisplay("{Width} x {Height}")]
public record Size
{
    /// <summary>
    /// Size that requires zero pixels.
    /// </summary>
    public static readonly Size Zero = new(0, 0);

    /// <summary>
    /// The width in pixels.
    /// </summary>
    public PixelUnit Width { get; }

    /// <summary>
    /// The height in pixels.
    /// </summary>
    public PixelUnit Height { get; }

    /// <summary>
    /// Creates a new size.
    /// </summary>
    /// <param name="width">The width in pixels (it must be greater or equal to zero).</param>
    /// <param name="height">The height in pixels (it must be greater or equal to zero).></param>
    public Size(PixelUnit width, PixelUnit height)
    {
        if (width.Value < 0.0f) throw new ArgumentException($"With must be greater or equal to zero, {width} found.");
        if (height.Value < 0.0f) throw new ArgumentException($"Height must be greater or equal to zero, {height} found.");

        Width = width;
        Height = height;
    }

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
        new(Point.Zero, this);

    public static Size operator +(Size s1, Size s2) => new(s1.Width + s2.Width, s1.Height + s2.Height);
    public static Size operator -(Size s1, Size s2) => new(s1.Width - s2.Width, s1.Height - s2.Height);

    /// <inheritdoc />
    public override string ToString() =>
        $"{Width.Value} x {Height.Value}";
}
